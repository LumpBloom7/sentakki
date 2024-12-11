namespace SimaiSharp.Internal.Errors
{
    public class UnterminatedSectionException : SimaiException
    {
        public UnterminatedSectionException(int line, int character) : base(line, character)
        {
        }
    }
}
