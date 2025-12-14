namespace SimaiSharp.Internal.SyntacticAnalysis
{
    public struct TimingChange
    {
        public float trueInheritedTempo;
        public float time;
        public float tempo;
        public float subdivisions;

        /// <summary>
        ///     Used in duration parsing.
        /// </summary>
        public float SecondsPerBar => tempo == 0 ? 0 : 60f / tempo;

        public float SecondsPerBeat => SecondsPerBar / ((subdivisions == 0 ? 4 : subdivisions) / 4);

        public bool TempoInherited;
        public bool IsAbsoluteTimingPoint;

        public void SetSeconds(float value)
        {
            tempo = 60f / value;
            subdivisions = 4;
            IsAbsoluteTimingPoint = true;
        }
    }
}
