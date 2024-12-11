using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimaiSharp.Internal;

namespace SimaiSharp
{
    /// <summary>
    /// <para>
    /// A wrapper for parsing Simai files
    /// </para>
    /// <para>
    /// This class extracts key-value data from a maidata file.
    /// For simai chart serialization, use <see cref="SimaiConvert"/>
    /// </para>
    /// </summary>
    public sealed class SimaiFile : IDisposable
    {
        private readonly StreamReader _simaiReader;

        public SimaiFile(FileSystemInfo file)
        {
            const int sampleSize = 64;

            var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);

            // Determine the encoding of the file
            byte[] buffer = new byte[64];
            int numCharsRead = fileStream.Read(buffer, 0, 64);
            var encoding = buffer[..numCharsRead].TryGetEncoding(sampleSize);

            // We've already read 64 chars, so we'll reset here.
            fileStream.Position = 0;

            _simaiReader = new StreamReader(fileStream, encoding);
        }

        public SimaiFile(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;

            _simaiReader = new StreamReader(stream);
        }

        public SimaiFile(Stream stream)
        {
            _simaiReader = new StreamReader(stream);
        }

        public SimaiFile(StreamReader reader)
        {
            _simaiReader = reader;
        }

        public IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs()
        {
            _simaiReader.BaseStream.Position = 0;

            string currentKey = string.Empty;
            var currentValue = new StringBuilder();

            while (!_simaiReader.EndOfStream)
            {
                string? line = _simaiReader.ReadLine();

                if (line == null)
                    break;

                if (line.StartsWith('&'))
                {
                    if (currentKey != string.Empty)
                    {
                        yield return new KeyValuePair<string, string>(currentKey, currentValue.ToString().TrimEnd());
                        currentValue.Clear();
                    }

                    string[] keyValuePair = line.Split('=', 2);
                    currentKey = keyValuePair[0][1..];
                    currentValue.AppendLine(keyValuePair[1]);
                }
                else
                {
                    currentValue.AppendLine(line);
                }
            }

            // Add the last entry
            yield return new KeyValuePair<string, string>(currentKey, currentValue.ToString().TrimEnd());
        }

        public string? GetValue(string key)
        {
            _simaiReader.BaseStream.Position = 0;

            string keyPart = $"&{key}=";
            int keyPartLength = keyPart.Length;

            var result = new StringBuilder();
            bool readingValue = false;

            while (!_simaiReader.EndOfStream)
            {
                string? line = _simaiReader.ReadLine();

                if (line == null)
                    break;

                if (line.StartsWith('&'))
                {
                    if (readingValue)
                        return result.ToString().TrimEnd();

                    // https://stackoverflow.com/questions/3120056/contains-is-faster-than-startswith
                    if (!line.StartsWith(keyPart, StringComparison.OrdinalIgnoreCase))
                        continue;

                    readingValue = true;
                    result.AppendLine(line[keyPartLength..]);
                }
                else if (readingValue)
                    result.AppendLine(line);
            }

            return readingValue ? result.ToString().TrimEnd() : null;
        }

        public void Dispose()
        {
            _simaiReader.Dispose();
        }
    }
}
