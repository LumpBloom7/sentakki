namespace SimaiSharp.Internal.SyntacticAnalysis
{
    public struct TimingChange
    {
        public double trueInheritedTempo;
        public double time;
        public double tempo;
        public double subdivisions;

        /// <summary>
        ///     Used in duration parsing.
        /// </summary>
        public double SecondsPerBar => tempo == 0 ? 0 : 60 / tempo;

        public double SecondsPerBeat => SecondsPerBar / ((subdivisions == 0 ? 4 : subdivisions) / 4);

        public bool TempoInherited;
        public bool IsAbsoluteTimingPoint;

        public void SetSeconds(double value)
        {
            tempo = 60 / value;
            subdivisions = 4;
            IsAbsoluteTimingPoint = true;
        }
    }
}
