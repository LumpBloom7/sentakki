using System;
using SimaiSharp.Internal.SyntacticAnalysis;

namespace SimaiSharp.Structures
{
    [Serializable]
    public sealed class MaiChart
    {
        public float? FinishTiming { get; internal set; }
        public NoteCollection[] NoteCollections { get; internal set; } = Array.Empty<NoteCollection>();
        public TimingChange[] TimingChanges { get; internal set; } = Array.Empty<TimingChange>();
    }
}
