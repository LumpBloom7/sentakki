using SimaiSharp.Structures;

namespace SimaiSharp.Internal.LexicalAnalysis
{
    internal enum TokenType
    {
        None,
        Tempo,
        Subdivision,

        /// <summary>
        ///     <para>Touch locations (A~E + 1~8) and tap locations (1~8)</para>
        ///     <para>
        ///         Takes either only a number (1 ~ 8) or a character (A ~ E) followed by a number (1 ~ 8 for A, B, D, E and 1 or
        ///         2 for C)
        ///     </para>
        /// </summary>
        Location,

        /// <summary>
        ///     Applies note styles and note types
        /// </summary>
        Decorator,

        /// <summary>
        ///     Takes a <see cref="SlideType" /> and target vertices
        /// </summary>
        Slide,

        /// <summary>
        ///     Usually denotes the length of a hold or a <see cref="SlidePath" />
        /// </summary>
        Duration,

        /// <summary>
        ///     Allows multiple slides to share the same parent note
        /// </summary>
        SlideJoiner,

        /// <summary>
        ///     Progresses the time by 1 beat
        /// </summary>
        TimeStep,

        EachDivider,
        EndOfFile
    }
}
