namespace SimaiSharp.Internal.Errors
{
    public class UnsupportedSyntaxException : SimaiException
    {
        /// <summary>
        ///	<para>
        ///		This is thrown when an unsupported syntax is encountered when attempting to tokenize or deserialize the simai file.
        /// </para>
        /// </summary>
        /// <param name="line">The line on which the error occurred</param>
        /// <param name="character">The first character involved in the error</param>
        public UnsupportedSyntaxException(int line, int character) : base(line, character)
        {
        }
    }
}
