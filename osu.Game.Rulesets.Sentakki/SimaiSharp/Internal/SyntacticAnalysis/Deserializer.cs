using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SimaiSharp.Internal.Errors;
using SimaiSharp.Internal.LexicalAnalysis;
using SimaiSharp.Internal.SyntacticAnalysis.States;
using SimaiSharp.Structures;

namespace SimaiSharp.Internal.SyntacticAnalysis
{
    internal sealed class Deserializer : IDisposable
    {
        private readonly MaiChart _chart = new();
        internal readonly IEnumerator<Token> enumerator;

        internal readonly List<TimingChange> timingChanges = new();
        private float _maxFinishTime;
        internal float currentTime;
        internal NoteCollection? currentNoteCollection;
        internal bool endOfFile;

        public Deserializer(IEnumerable<Token> sequence)
        {
            enumerator = sequence.GetEnumerator();
            timingChanges.Add(new TimingChange());
            currentNoteCollection = null;
            currentTime = 0;
            endOfFile = false;
        }

        public void Dispose()
        {
            enumerator.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MaiChart GetChart()
        {
            var noteCollections = new List<NoteCollection>();

            // Some readers (e.g. NoteReader) moves the enumerator automatically.
            // We can skip moving the pointer if that's satisfied.
            bool manuallyMoved = false;

            while (!endOfFile && (manuallyMoved || MoveNext()))
            {
                var token = enumerator.Current;
                manuallyMoved = false;

                switch (token.type)
                {
                    case TokenType.Tempo:
                    {
                        TempoReader.Process(this, token);
                        break;
                    }
                    case TokenType.Subdivision:
                    {
                        SubdivisionReader.Process(this, token);
                        break;
                    }
                    case TokenType.Location:
                    {
                        currentNoteCollection ??= new NoteCollection(currentTime);

                        if (token.lexeme.Span[0] == '0')
                        {
                            if (currentNoteCollection.eachStyle is not EachStyle.ForceBroken)
                                currentNoteCollection.eachStyle = EachStyle.ForceEach;
                            break;
                        }

                        var note = NoteReader.Process(this, token);
                        currentNoteCollection.Add(note);
                        manuallyMoved = true;

                        _maxFinishTime = Math.Max(_maxFinishTime, currentNoteCollection.time + note.GetVisibleDuration());
                        break;
                    }
                    case TokenType.TimeStep:
                    {
                        if (currentNoteCollection != null)
                        {
                            noteCollections.Add(currentNoteCollection);
                            currentNoteCollection = null;
                        }

                        currentTime += timingChanges[^1].SecondsPerBeat;
                    }
                    break;
                    case TokenType.EachDivider:
                    {
                        switch (token.lexeme.Span[0])
                        {
                            case '/':
                                break;

                            case '`':
                                if (currentNoteCollection != null)
                                    currentNoteCollection.eachStyle = EachStyle.ForceBroken;
                                break;
                        }
                    }
                    break;
                    case TokenType.Decorator:
                        throw new ScopeMismatchException(token.line, token.character,
                                                         ScopeMismatchException.ScopeType.Note);
                    case TokenType.Slide:
                        throw new ScopeMismatchException(token.line, token.character,
                                                         ScopeMismatchException.ScopeType.Note);
                    case TokenType.Duration:
                        throw new ScopeMismatchException(token.line, token.character,
                                                         ScopeMismatchException.ScopeType.Note |
                                                         ScopeMismatchException.ScopeType.Slide);
                    case TokenType.SlideJoiner:
                        throw new ScopeMismatchException(token.line, token.character,
                                                         ScopeMismatchException.ScopeType.Slide);
                    case TokenType.EndOfFile:
                        _chart.FinishTiming = currentTime;
                        break;
                    case TokenType.None:
                        break;
                    default:
                        throw new UnsupportedSyntaxException(token.line, token.character);
                }
            }

            _chart.FinishTiming ??= _maxFinishTime;

            if (currentNoteCollection != null)
            {
                noteCollections.Add(currentNoteCollection);
                currentNoteCollection = null;
            }

            _chart.NoteCollections = noteCollections.ToArray();
            _chart.TimingChanges = timingChanges.ToArray();

            return _chart;
        }

        /// <param name="startLocation">The starting location.</param>
        /// <param name="endLocation">The ending location.</param>
        /// <param name="direction">1: Right; -1: Left; Default: Shortest route.</param>
        /// <returns>The recommended ring type</returns>
        internal static SlideType DetermineRingType(Location startLocation,
                                                    Location endLocation,
                                                    int direction = 0)
        {
            switch (direction)
            {
                case 1:
                    return ((startLocation.index + 2) % 8 >= 4) ? SlideType.RingCcw : SlideType.RingCw;
                case -1:
                    return ((startLocation.index + 2) % 8 >= 4) ? SlideType.RingCw : SlideType.RingCcw;
                default:
                {
                    int difference = endLocation.index - startLocation.index;

                    int rotation = difference >= 0
                                       ? difference > 4 ? -1 : 1
                                       : difference < -4 ? 1 : -1;

                    return rotation > 0 ? SlideType.RingCw : SlideType.RingCcw;
                }
            }
        }

        internal static bool TryReadLocation(in Token token, out Location value)
        {
            bool isSensor = token.lexeme.Span[0] is >= 'A' and <= 'E';
            var indexRange = isSensor
                                 ? token.lexeme.Span[1..]
                                 : token.lexeme.Span[..];

            var group = NoteGroup.Tap;

            if (isSensor)
            {
                group = (NoteGroup)(token.lexeme.Span[0] - 'A' + 1);

                if (group == NoteGroup.CSensor)
                {
                    value = new Location(0, group);
                    return true;
                }
            }

            if (!int.TryParse(indexRange, out int indexInGroup))
            {
                value = new Location();
                return false;
            }

            // Convert from 1-indexed to 0-indexed
            value = new Location(indexInGroup - 1, group);
            return true;
        }

        public bool MoveNext()
        {
            endOfFile = !enumerator.MoveNext();
            return !endOfFile;
        }
    }
}
