using System;
namespace SimaiSharp.Internal.Errors
{
    [Serializable]
    public class SimaiException : Exception
    {
        public readonly int line;
        public readonly int character;

        ///  <param name="line">The line on which the error occurred</param>
        ///  <param name="character">The first character involved in the error</param>
        public SimaiException(int line, int character)
        {
            this.character = character;
            this.line = line;
        }
    }
}
