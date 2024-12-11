using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimaiSharp.Structures
{
    [Serializable]
    public class Note
    {
        [NonSerialized] public NoteCollection parentCollection;

        public Location location;
        public NoteStyles styles;
        public NoteAppearance appearance;
        public NoteType type;

        public float? length;

        public SlideMorph slideMorph;
        public List<SlidePath> slidePaths;

        public Note(NoteCollection parentCollection)
        {
            this.parentCollection = parentCollection;
            slidePaths = new List<SlidePath>();
            location = default;
            styles = NoteStyles.None;
            appearance = NoteAppearance.Default;
            type = NoteType.Tap;
            length = null;
            slideMorph = SlideMorph.FadeIn;
        }

        public bool IsEx => (styles & NoteStyles.Ex) != 0;

        public bool IsStar => appearance >= NoteAppearance.ForceStar ||
                              (slidePaths.Count > 0 && appearance is not NoteAppearance.ForceNormal);

        public float GetVisibleDuration()
        {
            float baseValue = length ?? 0;

            if (slidePaths is { Count: > 0 })
                baseValue = slidePaths.Max(s => s.delay + s.duration);

            return baseValue;
        }

        public void WriteTo(StringWriter writer)
        {
            writer.Write(location.ToString());

            // decorations
            if ((styles & NoteStyles.Ex) != 0)
                writer.Write('x');

            if ((styles & NoteStyles.Mine) != 0)
                writer.Write('m');

            // types
            if (type == NoteType.ForceInvalidate)
                writer.Write(slideMorph == SlideMorph.FadeIn ? '?' : '!');

            switch (appearance)
            {
                case NoteAppearance.ForceNormal:
                    writer.Write('@');
                    break;
                case NoteAppearance.ForceStarSpinning:
                    writer.Write("$$");
                    break;
                case NoteAppearance.ForceStar:
                    writer.Write('$');
                    break;
            }

            if (length.HasValue)
                writer.Write($"h[#{length.Value:0.0000000}]");

            for (int i = 0; i < slidePaths.Count; i++)
            {
                if (i > 0)
                    writer.Write('*');

                var slidePath = slidePaths[i];
                slidePath.WriteTo(writer);
            }
        }
    }
}
