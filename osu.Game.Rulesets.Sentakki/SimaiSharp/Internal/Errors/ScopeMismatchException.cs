using System;
namespace SimaiSharp.Internal.Errors
{
    public class ScopeMismatchException : SimaiException
    {
        public readonly ScopeType correctScope;

        public ScopeMismatchException(int line, int character, ScopeType correctScope) : base(line, character)
        {
            this.correctScope = correctScope;
        }

        [Flags]
        public enum ScopeType
        {
            Note = 1,
            Slide = 1 << 1,
            Global = 1 << 2
        }
    }
}
