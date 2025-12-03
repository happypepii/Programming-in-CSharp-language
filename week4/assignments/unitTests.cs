// UNIT TESTS using xUnit
using System.Collections.Generic;
using TokenProcessingFramework;
using Xunit;

namespace TokenProcessingFramework.Tests;

public class ParagraphDetectingTokenReaderDecoratorTests
{
    private class TestTokenReader : ITokenReader
    {
        private readonly Queue<Token> _q;
        public TestTokenReader(IEnumerable<Token> tokens)
        {
            _q = new Queue<Token>(tokens);
        }

        public Token ReadToken() => _q.Count > 0 ? _q.Dequeue() : new Token(TokenType.EndOfInput);
    }

    private static List<Token> ReadAll(ITokenReader reader)
    {
        var outList = new List<Token>();
        Token t;
        do
        {
            t = reader.ReadToken();
            outList.Add(t);
        } while (t.Type != TokenType.EndOfInput);
        return outList;
    }

    [Fact]
    public void No_newlines_forwards_words_unchanged()
    {
        // CONTRACT: Non-EOL tokens are forwarded unchanged
        // INPUT:  [Word("a"), Word("b")]
        // EXPECT: [Word("a"), Word("b"), EndOfInput]
        var src = new TestTokenReader(new[] { new Token("a"), new Token("b") });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[] { new Token("a"), new Token("b"), new Token(TokenType.EndOfInput) };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Single_eol_within_words_is_consumed_as_whitespace()
    {
        // CONTRACT: A single EOL is consumed as whitespace (like TextJustifier behavior)
        // → It separates words but does NOT produce an EOL token
        // INPUT:  [Word("Hello"), EOL, Word("World")]
        // EXPECT: [Word("Hello"), Word("World"), EndOfInput]  ← EOL consumed!
        var tokens = new[]
        {
            new Token(TokenType.Word, "Hello"),
            new Token(TokenType.EndOfLine),
            new Token(TokenType.Word, "World")
        };
        var reader = new ParagraphDetectingTokenReaderDecorator(new TestTokenReader(tokens));

        var result = ReadAll(reader);
        Assert.Equal(3, result.Count);
        Assert.Equal(TokenType.Word, result[0].Type);
        Assert.Equal("Hello", result[0].Value);
        Assert.Equal(TokenType.Word, result[1].Type);
        Assert.Equal("World", result[1].Value);
        Assert.Equal(TokenType.EndOfInput, result[2].Type);
    }

    [Fact]
    public void Double_eol_between_words_signals_paragraph_end()
    {
        // CONTRACT: Two consecutive EOLs (≥2) = paragraph boundary → emit EndOfParagraph token
        // INPUT:  [Word("a"), EOL, EOL, Word("b")]
        // EXPECT: [Word("a"), EndOfParagraph, Word("b"), EndOfInput]
        var src = new TestTokenReader(new Token[]
        {
            new Token("a"),
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine),
            new Token("b")
        });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[]
        {
            new Token("a"),
            new Token(TokenType.EndOfParagraph),
            new Token("b"),
            new Token(TokenType.EndOfInput)
        };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Multiple_consecutive_eols_collapsed_to_single_paragraph_marker()
    {
        // CONTRACT: Any run of ≥2 consecutive EOLs becomes exactly ONE EndOfParagraph token
        // Multiple EOLs don't produce multiple EndOfParagraph tokens (collapse behavior)
        // INPUT:  [Word("a"), EOL, EOL, EOL, Word("b")]
        // EXPECT: [Word("a"), EndOfParagraph, Word("b"), EndOfInput]  ← ONE EndOfParagraph, not three
        var src = new TestTokenReader(new Token[]
        {
            new Token("a"),
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine),
            new Token("b")
        });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[]
        {
            new Token("a"),
            new Token(TokenType.EndOfParagraph),
            new Token("b"),
            new Token(TokenType.EndOfInput)
        };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Trailing_eols_signal_end_of_last_paragraph()
    {
        // CONTRACT: EOLs at document end represent the boundary of the last paragraph
        // INPUT:  [Word("a"), EOL, EOL]  (no word after)
        // EXPECT: [Word("a"), EndOfParagraph, EndOfInput]
        var src = new TestTokenReader(new Token[]
        {
            new Token("a"),
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine)
        });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[]
        {
            new Token("a"),
            new Token(TokenType.EndOfParagraph),
            new Token(TokenType.EndOfInput)
        };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Leading_eols_at_document_start_are_ignored()
    {
        // CONTRACT: Leading EOLs at document start have no previous paragraph → ignore them
        // Do NOT emit EndOfParagraph at the start (that would signal a non-existent previous paragraph)
        // INPUT:  [EOL, EOL, Word("a")]
        // EXPECT: [Word("a"), EndOfInput]  ← Leading EOLs are consumed silently
        var src = new TestTokenReader(new Token[]
        {
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine),
            new Token("a")
        });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[]
        {
            new Token("a"),
            new Token(TokenType.EndOfInput)
        };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Empty_input_returns_only_end_of_input()
    {
        // CONTRACT: No tokens → only EndOfInput
        // INPUT:  []
        // EXPECT: [EndOfInput]
        var src = new TestTokenReader(System.Array.Empty<Token>());
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[] { new Token(TokenType.EndOfInput) };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Only_eols_at_start_followed_by_end_of_input()
    {
        // CONTRACT: Leading EOLs with no content after → consumed, EndOfInput follows
        // INPUT:  [EOL, EOL]
        // EXPECT: [EndOfInput]  ← Leading EOLs ignored, no content, no EndOfParagraph
        var src = new TestTokenReader(new Token[]
        {
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine)
        });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[] { new Token(TokenType.EndOfInput) };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Multiple_paragraphs_separated_by_double_eols()
    {
        // CONTRACT: Multiple paragraphs separated by paragraph boundaries
        // INPUT:  [Word("a"), EOL, EOL, Word("b"), EOL, EOL, Word("c")]
        // EXPECT: [Word("a"), EndOfParagraph, Word("b"), EndOfParagraph, Word("c"), EndOfInput]
        var src = new TestTokenReader(new Token[]
        {
            new Token("a"),
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine),
            new Token("b"),
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine),
            new Token("c")
        });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[]
        {
            new Token("a"),
            new Token(TokenType.EndOfParagraph),
            new Token("b"),
            new Token(TokenType.EndOfParagraph),
            new Token("c"),
            new Token(TokenType.EndOfInput)
        };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Paragraph_with_internal_line_breaks_and_boundary()
    {
        // CONTRACT: Single EOLs within paragraph are CONSUMED (not forwarded); double EOLs mark boundary
        // INPUT:  [Word("hello"), EOL, Word("world"), EOL, EOL, Word("foo")]
        // EXPECT: [Word("hello"), Word("world"), EndOfParagraph, Word("foo"), EndOfInput]
        //         ↑ Single EOL is consumed as whitespace
        var src = new TestTokenReader(new Token[]
        {
            new Token("hello"),
            new Token(TokenType.EndOfLine),
            new Token("world"),
            new Token(TokenType.EndOfLine),
            new Token(TokenType.EndOfLine),
            new Token("foo")
        });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[]
        {
            new Token("hello"),
            new Token("world"),
            new Token(TokenType.EndOfParagraph),
            new Token("foo"),
            new Token(TokenType.EndOfInput)
        };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Single_trailing_eol_is_ignored()
    {
        // CONTRACT: A single trailing EOL at EOF is consumed (not a paragraph boundary)
        // INPUT:  [Word("hello"), EOL]
        // EXPECT: [Word("hello"), EndOfInput]  ← Single EOL at end is consumed
        var src = new TestTokenReader(new Token[]
        {
            new Token("hello"),
            new Token(TokenType.EndOfLine)
        });
        var deco = new ParagraphDetectingTokenReaderDecorator(src);
        var actual = ReadAll(deco);
        var expected = new[]
        {
            new Token("hello"),
            new Token(TokenType.EndOfInput)
        };
        Assert.Equal(expected, actual);
    }
}