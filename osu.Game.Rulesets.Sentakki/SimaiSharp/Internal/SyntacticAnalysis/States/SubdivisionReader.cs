using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using SimaiSharp.Internal.Errors;
using SimaiSharp.Internal.LexicalAnalysis;

namespace SimaiSharp.Internal.SyntacticAnalysis.States
{
    internal static class SubdivisionReader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Process(Deserializer parent, Token token)
        {
            if (token.lexeme.Span[0] == '#')
            {
                if (!float.TryParse(token.lexeme.Span[1..],
                                    NumberStyles.Any,
                                    CultureInfo.InvariantCulture,
                                    out float explicitTempo))
                    throw new UnexpectedCharacterException(token.line, token.character + 1, "0~9, or \".\"");

                var newTimingChange = parent!.timingChanges[^1];
                newTimingChange.SetSeconds(explicitTempo);
                newTimingChange.time = parent.currentTime;

                if (Math.Abs(parent!.timingChanges[^1].time - parent.currentTime) <= float.Epsilon)
                {
                    parent.timingChanges.RemoveAt(parent.timingChanges.Count - 1);
                }
                else
                {
                    newTimingChange.TempoInherited = true;
                }

                parent.timingChanges.Add(newTimingChange);
                return;
            }

            if (!float.TryParse(token.lexeme.Span,
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out float subdivision)) throw new UnexpectedCharacterException(token.line, token.character, "0~9, or \".\"");

            {
                var newTimingChange = parent!.timingChanges[^1];
                newTimingChange.subdivisions = subdivision;
                newTimingChange.time = parent.currentTime;

                if (Math.Abs(parent!.timingChanges[^1].time - parent.currentTime) <= float.Epsilon)
                {
                    parent.timingChanges.RemoveAt(parent.timingChanges.Count - 1);
                }
                else
                {
                    newTimingChange.TempoInherited = true;
                }

                parent.timingChanges.Add(newTimingChange);
            }
        }
    }
}
