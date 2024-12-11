namespace SimaiSharp.Internal.Errors
{
    internal class UnexpectedCharacterException : SimaiException
    {
        public readonly string expected;

        ///  <summary>
        ///  <para>
        /// 		This is thrown when reading a character that is not fit for the expected syntax
        ///  </para>
        ///  <para>
        /// 		This issue is commonly caused by a typo or a syntax error.
        ///  </para>
        ///  
        ///  </summary>
        ///  <param name="line">The line on which the error occurred</param>
        ///  <param name="character">The first character involved in the error</param>
        ///  <param name="expected">The expected syntax</param>	
        public UnexpectedCharacterException(int line, int character, string expected) : base(line, character)
        {
            this.expected = expected;
        }
    }
}
