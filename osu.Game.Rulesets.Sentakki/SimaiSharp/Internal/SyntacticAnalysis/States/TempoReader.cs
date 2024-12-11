using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.XPath;
using SimaiSharp.Internal.Errors;
using SimaiSharp.Internal.LexicalAnalysis;

namespace SimaiSharp.Internal.SyntacticAnalysis.States
{
    internal static class TempoReader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Process(Deserializer parent, Token token)
        {
            if (!float.TryParse(token.lexeme.Span,
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out float tempo))
                throw new UnexpectedCharacterException(token.line, token.character, "0~9, or \".\"");

            var newTimingChange = parent.timingChanges[^1];
            newTimingChange.tempo = tempo;
            newTimingChange.time = parent.currentTime;

            if (Math.Abs(parent.timingChanges[^1].time - parent.currentTime) <= float.Epsilon)
                parent.timingChanges.RemoveAt(parent.timingChanges.Count - 1);

            parent.timingChanges.Add(newTimingChange);
        }
    }
}
