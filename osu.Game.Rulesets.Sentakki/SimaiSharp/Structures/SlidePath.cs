using System;
using System.Collections.Generic;
using System.IO;

namespace SimaiSharp.Structures
{
    [Serializable]
    public class SlidePath
    {
        public Location startLocation;
        public List<SlideSegment> segments;

        /// <summary>
        ///     The intro delay of a slide before it starts moving.
        /// </summary>
        public double delay;

        public double duration;

        public NoteType type;

        public SlidePath(List<SlideSegment> segments)
        {
            this.segments = segments;
            startLocation = default;
            delay = 0;
            duration = 0;
            type = NoteType.Slide;
        }

        public void WriteTo(StringWriter writer)
        {
            foreach (var segment in segments)
                segment.WriteTo(writer, startLocation);

            if (type == NoteType.Break)
                writer.Write('b');

            writer.Write($"[{delay:0.0000000}##{duration:0.0000000}]");
        }
    }
}
