using System;

namespace SimaiSharp.Structures
{
    [Flags]
    public enum NoteStyles
    {
        None = 0,
        Ex = 1 << 0,
        Fireworks = 1 << 1,
        Mine = 1 << 2
    }
}
