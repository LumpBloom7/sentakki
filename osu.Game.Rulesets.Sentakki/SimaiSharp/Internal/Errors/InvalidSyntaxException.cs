namespace SimaiSharp.Internal.Errors
{
    public class InvalidSyntaxException : SimaiException
    {
        public InvalidSyntaxException(int line, int character) : base(line, character)
        {
        }
    }
}
