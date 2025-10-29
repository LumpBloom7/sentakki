using System;
using System.Collections.Generic;
using System.IO;

namespace SimaiSharp.Structures
{
    public class SlideSegment
    {
        /// <summary>
        ///     Describes the target buttons
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public List<Location> vertices;

        public SlideType slideType;

        public SlideSegment(List<Location>? vertices = null)
        {
            this.vertices = vertices ?? new List<Location>();
            slideType = SlideType.StraightLine;
        }

        public void WriteTo(StringWriter writer, Location startLocation)
        {
            switch (slideType)
            {
                case SlideType.StraightLine:
                    writer.Write($"-{vertices[0]}");
                    break;
                case SlideType.RingCw:
                    writer.Write((startLocation.index + 2) % 8 >= 4 ? $"<{vertices[0]}" : $">{vertices[0]}");
                    break;
                case SlideType.RingCcw:
                    writer.Write((startLocation.index + 2) % 8 >= 4 ? $"<{vertices[0]}" : $">{vertices[0]}");
                    break;
                case SlideType.Fold:
                    writer.Write($"v{vertices[0]}");
                    break;
                case SlideType.CurveCw:
                    writer.Write($"q{vertices[0]}");
                    break;
                case SlideType.CurveCcw:
                    writer.Write($"pp{vertices[0]}");
                    break;
                case SlideType.ZigZagS:
                    writer.Write($"s{vertices[0]}");
                    break;
                case SlideType.ZigZagZ:
                    writer.Write($"z{vertices[0]}");
                    break;
                case SlideType.EdgeFold:
                    writer.Write($"V{vertices[0]}{vertices[1]}");
                    break;
                case SlideType.EdgeCurveCw:
                    writer.Write($"qq{vertices[0]}");
                    break;
                case SlideType.EdgeCurveCcw:
                    writer.Write($"pp{vertices[0]}");
                    break;
                case SlideType.Fan:
                    writer.Write($"w{vertices[0]}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
