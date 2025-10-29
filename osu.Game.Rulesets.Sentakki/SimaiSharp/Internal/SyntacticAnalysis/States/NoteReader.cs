using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using SimaiSharp.Internal.Errors;
using SimaiSharp.Internal.LexicalAnalysis;
using SimaiSharp.Structures;

namespace SimaiSharp.Internal.SyntacticAnalysis.States
{
    internal static class NoteReader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Note Process(Deserializer parent, Token identityToken)
        {
            if (!Deserializer.TryReadLocation(in identityToken, out var noteLocation))
                throw new InvalidSyntaxException(identityToken.line, identityToken.character);

            var currentNote = new Note(parent.currentNoteCollection!)
            {
                location = noteLocation
            };

            var overrideTiming = new TimingChange
            {
                tempo = parent!.timingChanges[^1].tempo
            };

            if (noteLocation.group != NoteGroup.Tap)
                currentNote.type = NoteType.Touch;

            // Some readers (e.g. NoteReader) moves the enumerator automatically.
            // We can skip moving the pointer if that's satisfied.
            bool manuallyMoved = false;

            while (!parent.endOfFile && (manuallyMoved || parent.MoveNext()))
            {
                var token = parent.enumerator.Current;
                manuallyMoved = false;

                switch (token.type)
                {
                    case TokenType.Tempo:
                    case TokenType.Subdivision:
                        throw new ScopeMismatchException(token.line, token.character, ScopeMismatchException.ScopeType.Global);

                    case TokenType.Decorator:
                    {
                        DecorateNote(in token, ref currentNote);
                        break;
                    }

                    case TokenType.Slide:
                    {
                        if (currentNote.type == NoteType.Hold) currentNote.length = overrideTiming.SecondsPerBeat;

                        var slide = SlideReader.Process(parent, in currentNote, in token, in overrideTiming);
                        manuallyMoved = true;

                        currentNote.slidePaths.Add(slide);
                        break;
                    }

                    case TokenType.Duration:
                    {
                        ReadDuration(parent.timingChanges[^1], in token, ref currentNote);
                        break;
                    }

                    case TokenType.SlideJoiner:
                        throw new ScopeMismatchException(token.line, token.character, ScopeMismatchException.ScopeType.Slide);

                    case TokenType.TimeStep:
                    case TokenType.EachDivider:
                    case TokenType.EndOfFile:
                    case TokenType.Location:
                        // note terminates here
                        return currentNote;

                    case TokenType.None:
                        break;

                    default:
                        throw new UnsupportedSyntaxException(token.line, token.character);
                }
            }

            return currentNote;
        }

        private static void DecorateNote(in Token token, ref Note note)
        {
            switch (token.lexeme.Span[0])
            {
                case 'f':
                    note.styles |= NoteStyles.Fireworks;
                    return;
                case 'b' when note.type is not NoteType.ForceInvalidate:
                    // always override note type
                    note.type = NoteType.Break;
                    return;
                case 'x':
                    note.styles |= NoteStyles.Ex;
                    return;
                case 'm':
                    note.styles |= NoteStyles.Mine;
                    return;
                case 'h':
                    if (note.type != NoteType.Break && note.type != NoteType.ForceInvalidate)
                        note.type = NoteType.Hold;
                    note.length ??= 0;
                    return;
                case '?':
                    note.type = NoteType.ForceInvalidate;
                    note.slideMorph = SlideMorph.FadeIn;
                    return;
                case '!':
                    note.type = NoteType.ForceInvalidate;
                    note.slideMorph = SlideMorph.SuddenIn;
                    return;
                case '@':
                    note.appearance = NoteAppearance.ForceNormal;
                    return;
                case '$':
                    note.appearance = note.appearance is NoteAppearance.ForceStar
                                          ? NoteAppearance.ForceStarSpinning
                                          : NoteAppearance.ForceStar;
                    return;
            }
        }

        private static void ReadDuration(TimingChange timing, in Token token, ref Note note)
        {
            if (note.type != NoteType.Break)
                note.type = NoteType.Hold;

            int indexOfHash = token.lexeme.Span.IndexOf('#');

            if (indexOfHash == 0)
            {
                if (!float.TryParse(token.lexeme.Span[1..],
                                    NumberStyles.Any,
                                    CultureInfo.InvariantCulture,
                                    out float explicitValue))
                    throw new UnexpectedCharacterException(token.line, token.character + 1, "0~9, or \".\"");

                note.length = explicitValue;
                return;
            }

            if (indexOfHash != -1)
            {
                if (!float.TryParse(token.lexeme.Span[..indexOfHash],
                                    NumberStyles.Any,
                                    CultureInfo.InvariantCulture,
                                    out float tempo))
                    throw new UnexpectedCharacterException(token.line, token.character + 1, "0~9, or \".\"");

                timing.tempo = tempo;
            }

            int indexOfSeparator = token.lexeme.Span.IndexOf(':');
            if (!float.TryParse(token.lexeme.Span[(indexOfHash + 1)..indexOfSeparator],
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out float nominator))
                throw new UnexpectedCharacterException(token.line, token.character, "0~9, or \".\"");

            if (!float.TryParse(token.lexeme.Span[(indexOfSeparator + 1)..],
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out float denominator))
                throw new UnexpectedCharacterException(token.line, token.character + indexOfSeparator + 1, "0~9, or \".\"");

            note.length = timing.SecondsPerBar / (nominator / 4) * denominator;
        }
    }
}
