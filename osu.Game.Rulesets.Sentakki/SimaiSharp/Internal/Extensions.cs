using System;
using System.Text;

namespace SimaiSharp.Internal
{
    public static class Extensions
    {
        public static string RemoveLineEndings(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            const char lineSeparator = (char)0x2028;
            const char paragraphSeparator = (char)0x2029;

            return value.Replace("\r\n", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty)
                        .Replace(lineSeparator.ToString(), string.Empty)
                        .Replace(paragraphSeparator.ToString(), string.Empty);
        }

        /// <summary>
        ///     Function to detect the encoding for UTF-7, UTF-8/16/32 (bom, no bom, little and big endian),
        ///     and local default codepage, and potentially other codepages.
        /// </summary>
        /// <param name="textBytes">The input text</param>
        /// <param name="taster">Number of bytes to check of the file (to save processing).</param>
        /// <remarks>https://stackoverflow.com/questions/1025332/determine-a-strings-encoding-in-c-sharp</remarks>
        public static Encoding TryGetEncoding(this byte[] textBytes, int taster)
        {
            switch (textBytes.Length)
            {
                //////////////// First check the low hanging fruit by checking if a
                //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
                case >= 4 when textBytes[0] == 0x00 && textBytes[1] == 0x00 && textBytes[2] == 0xFE &&
                               textBytes[3] == 0xFF:
                    return Encoding.GetEncoding("utf-32BE"); // UTF-32, big-endian 
                case >= 4 when textBytes[0] == 0xFF && textBytes[1] == 0xFE && textBytes[2] == 0x00 &&
                               textBytes[3] == 0x00:
                    return Encoding.UTF32; // UTF-32, little-endian
                case >= 2 when textBytes[0] == 0xFE && textBytes[1] == 0xFF:
                    return Encoding.BigEndianUnicode; // UTF-16, big-endian
                case >= 2 when textBytes[0] == 0xFF && textBytes[1] == 0xFE:
                    return Encoding.Unicode; // UTF-16, little-endian
                case >= 3 when textBytes[0] == 0xEF && textBytes[1] == 0xBB && textBytes[2] == 0xBF:
                    return Encoding.UTF8; // UTF-8
                case >= 3 when textBytes[0] == 0x2b && textBytes[1] == 0x2f && textBytes[2] == 0x76:
                    return Encoding.UTF7; // UTF-7
            }


            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            if (taster == 0 || taster > textBytes.Length)
                taster = textBytes.Length; // Taster size can't be bigger than the filesize obviously.


            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: https://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: https://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < taster - 4)
            {
                switch (textBytes[i])
                {
                    case <= 0x7F:
                        i += 1;
                        continue; // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                    case >= 0xC2 and < 0xE0 when textBytes[i + 1] >= 0x80 && textBytes[i + 1] < 0xC0:
                        i += 2;
                        utf8 = true;
                        continue;
                }

                if (textBytes[i] >= 0xE0 && textBytes[i] < 0xF0 && textBytes[i + 1] >= 0x80 &&
                    textBytes[i + 1] < 0xC0 && textBytes[i + 2] >= 0x80 &&
                    textBytes[i + 2] < 0xC0)
                {
                    i += 3;
                    utf8 = true;
                    continue;
                }

                if (textBytes[i] >= 0xF0 && textBytes[i] < 0xF5 && textBytes[i + 1] >= 0x80 &&
                    textBytes[i + 1] < 0xC0 && textBytes[i + 2] >= 0x80 &&
                    textBytes[i + 2] < 0xC0 && textBytes[i + 3] >= 0x80 && textBytes[i + 3] < 0xC0)
                {
                    i += 4;
                    utf8 = true;
                    continue;
                }

                utf8 = false;
                break;
            }

            if (utf8) return Encoding.UTF8;


            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            const double
                threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < taster; n += 2)
                if (textBytes[n] == 0)
                    count++;
            if ((double)count / taster > threshold) return Encoding.BigEndianUnicode;

            count = 0;
            for (int n = 1; n < taster; n += 2)
                if (textBytes[n] == 0)
                    count++;
            if ((double)count / taster > threshold) return Encoding.Unicode; // (little-endian)


            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < taster - 9; n++)
            {
                if (((textBytes[n + 0] != 'c' && textBytes[n + 0] != 'C') ||
                     (textBytes[n + 1] != 'h' && textBytes[n + 1] != 'H') ||
                     (textBytes[n + 2] != 'a' && textBytes[n + 2] != 'A') ||
                     (textBytes[n + 3] != 'r' && textBytes[n + 3] != 'R') ||
                     (textBytes[n + 4] != 's' && textBytes[n + 4] != 'S') ||
                     (textBytes[n + 5] != 'e' && textBytes[n + 5] != 'E') ||
                     (textBytes[n + 6] != 't' && textBytes[n + 6] != 'T') || textBytes[n + 7] != '=') &&
                    ((textBytes[n + 0] != 'e' && textBytes[n + 0] != 'E') ||
                     (textBytes[n + 1] != 'n' && textBytes[n + 1] != 'N') ||
                     (textBytes[n + 2] != 'c' && textBytes[n + 2] != 'C') ||
                     (textBytes[n + 3] != 'o' && textBytes[n + 3] != 'O') ||
                     (textBytes[n + 4] != 'd' && textBytes[n + 4] != 'D') ||
                     (textBytes[n + 5] != 'i' && textBytes[n + 5] != 'I') ||
                     (textBytes[n + 6] != 'n' && textBytes[n + 6] != 'N') ||
                     (textBytes[n + 7] != 'g' && textBytes[n + 7] != 'G') ||
                     textBytes[n + 8] != '=')) continue;

                if (textBytes[n + 0] == 'c' || textBytes[n + 0] == 'C') n += 8;
                else n += 9;
                if (textBytes[n] == '"' || textBytes[n] == '\'') n++;
                int oldN = n;
                while (n < taster && (textBytes[n] == '_' || textBytes[n] == '-' ||
                                      (textBytes[n] >= '0' && textBytes[n] <= '9') ||
                                      (textBytes[n] >= 'a' && textBytes[n] <= 'z') ||
                                      (textBytes[n] >= 'A' && textBytes[n] <= 'Z')))
                    n++;

                byte[] nb = new byte[n - oldN];
                Array.Copy(textBytes, oldN, nb, 0, n - oldN);
                try
                {
                    string internalEnc = Encoding.ASCII.GetString(nb);
                    return Encoding.GetEncoding(internalEnc);
                }
                catch
                {
                    // If C# doesn't recognize the name of the encoding, break.
                    break;
                }
            }


            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: https://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            return Encoding.Default;
        }
    }
}
