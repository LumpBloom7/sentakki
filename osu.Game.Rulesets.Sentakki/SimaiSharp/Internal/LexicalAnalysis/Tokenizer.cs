using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SimaiSharp.Internal.Errors;

namespace SimaiSharp.Internal.LexicalAnalysis
{
    internal sealed class Tokenizer
    {
        private const char Space = (char)0x0020;
        private const char EnSpace = (char)0x2002;
        private const char PunctuationSpace = (char)0x2008;
        private const char IdeographicSpace = (char)0x3000;

        private const char LineSeparator = (char)0x2028;
        private const char ParagraphSeparator = (char)0x2029;

        private const char EndOfFileChar = 'E';

        private static readonly HashSet<char> EachDividerChars = new()
        {
            '/', '`'
        };

        private static readonly HashSet<char> DecoratorChars = new()
        {
            'f', 'b', 'x', 'h', 'm',
            '!', '?',
            '@', '$'
        };

        private static readonly HashSet<char> SlideChars = new()
        {
            '-',
            '>', '<', '^',
            'p', 'q',
            'v', 'V',
            's', 'z',
            'w'
        };

        private static readonly HashSet<char> SeparatorChars = new()
        {
            '\r', '\t',
            LineSeparator,
            ParagraphSeparator,
            Space,
            EnSpace,
            PunctuationSpace,
            IdeographicSpace
        };

        private readonly ReadOnlyMemory<char> _sequence;
        private int _current;
        private int _charIndex;
        private int _line = 1;

        private int _start;

        public Tokenizer(string sequence)
        {
            _sequence = sequence.AsMemory();
        }

        private bool IsAtEnd => _current >= _sequence.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Token> GetTokens()
        {
            while (!IsAtEnd)
            {
                _start = _current;

                var nextToken = ScanToken();

                if (nextToken.HasValue)
                    yield return nextToken.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Token? ScanToken()
        {
            _charIndex++;
            char c = Advance();
            switch (c)
            {
                case ',':
                    return CompileToken(TokenType.TimeStep);

                case '(':
                    return CompileSectionToken(TokenType.Tempo, '(', ')');
                case '{':
                    return CompileSectionToken(TokenType.Subdivision, '{', '}');
                case '[':
                    return CompileSectionToken(TokenType.Duration, '[', ']');

                case var _ when TryScanLocationToken(out int length):
                    _current += length - 1;
                    return CompileToken(TokenType.Location);

                case var _ when DecoratorChars.Contains(c):
                    return CompileToken(TokenType.Decorator);

                case var _ when IsReadingSlideDeclaration(out int length):
                    _current += length - 1;
                    return CompileToken(TokenType.Slide);

                case '*':
                    return CompileToken(TokenType.SlideJoiner);

                case var _ when EachDividerChars.Contains(c):
                    return CompileToken(TokenType.EachDivider);

                case var _ when SeparatorChars.Contains(c):
                    // Ignore whitespace.
                    return null;

                case '\n':
                    _line++;
                    _charIndex = 0;
                    return null;

                case 'E':
                    return CompileToken(TokenType.EndOfFile);

                case '|':
                    {
                        if (Peek() != '|')
                            throw new UnexpectedCharacterException(_line, _charIndex, "|");

                        while (Peek() != '\n' && !IsAtEnd)
                            Advance();

                        return null;
                    }

                default:
                    throw new UnsupportedSyntaxException(_line, _charIndex);
            }
        }

        private bool TryScanLocationToken(out int length)
        {
            char firstLocationChar = PeekPrevious();

            if (IsButtonLocation(firstLocationChar))
            {
                length = 1;
                return true;
            }

            length = 0;

            if (!IsSensorLocation(firstLocationChar))
                return false;

            char secondLocationChar = Peek();

            if (IsButtonLocation(secondLocationChar))
            {
                length = 2;
                return true;
            }

            if (firstLocationChar == 'C')
            {
                length = 1;
                return true;
            }

            bool secondCharIsEmpty = SeparatorChars.Contains(secondLocationChar) ||
                                    secondLocationChar is '\n' or '\0';

            // This is the notation for EOF.
            if (firstLocationChar == EndOfFileChar && secondCharIsEmpty)
                return false;

            throw new UnexpectedCharacterException(_line, _charIndex, "1, 2, 3, 4, 5, 6, 7, 8");
        }

        private bool IsReadingSlideDeclaration(out int length)
        {
            if (!SlideChars.Contains(PeekPrevious()))
            {
                length = 0;
                return false;
            }

            char nextChar = Peek();

            length = nextChar is 'p' or 'q' ? 2 : 1;
            return true;
        }

        private Token? CompileSectionToken(TokenType tokenType, char initiator, char terminator)
        {
            _start++;
            while (Peek() != terminator)
            {
                if (IsAtEnd || Peek() == initiator)
                    throw new UnterminatedSectionException(_line, _charIndex);

                Advance();
            }

            var token = CompileToken(tokenType);

            // The terminator.
            Advance();

            return token;
        }

        private Token CompileToken(TokenType type)
        {
            var text = _sequence[_start.._current];
            return new Token(type, text, _line, _charIndex);
        }

        private static bool IsSensorLocation(char value)
        {
            return value is >= 'A' and <= 'E';
        }

        private static bool IsButtonLocation(char value)
        {
            return value is >= '0' and <= '8';
        }

        /// <summary>
        ///     Returns the <see cref="_current" /> glyph, and increments by one.
        /// </summary>
        private char Advance()
        {
            return _sequence.Span[_current++];
        }

        /// <summary>
        ///     Returns the <see cref="_current" /> glyph without incrementing.
        /// </summary>
        private char Peek()
        {
            return IsAtEnd ? default : _sequence.Span[_current];
        }

        /// <summary>
        ///     Returns the last glyph without decrementing.
        /// </summary>
        private char PeekPrevious()
        {
            return _current == 0 ? default : _sequence.Span[_current - 1];
        }
    }
}
