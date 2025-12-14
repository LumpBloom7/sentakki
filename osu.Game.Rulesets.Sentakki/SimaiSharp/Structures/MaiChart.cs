using System;
using SimaiSharp.Internal.SyntacticAnalysis;

namespace SimaiSharp.Structures
{
    [Serializable]
    public sealed class MaiChart
    {
        public float? FinishTiming { get; set; }
        public NoteCollection[] NoteCollections { get; set; } = Array.Empty<NoteCollection>();
        public TimingChange[] TimingChanges { get; set; } = Array.Empty<TimingChange>();
    }
}
