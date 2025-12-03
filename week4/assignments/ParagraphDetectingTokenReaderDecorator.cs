namespace TokenProcessingFramework;

public record class ParagraphDetectingTokenReaderDecorator(ITokenReader Reader) : ITokenReader
{
    private Token? _bufferedToken = null;
    private bool _hasSeenWord = false;

    public Token ReadToken()
    {
        
        if (_bufferedToken is not null)
        {
            var buffered = _bufferedToken.Value;
            _bufferedToken = null;
            return buffered;
        }

        
        int consecutiveEOLs = 0;
        Token current;
        
        while ((current = Reader.ReadToken()).Type == TokenType.EndOfLine)
        {
            consecutiveEOLs++;
        }

        // Case 1: no EOF
        if (consecutiveEOLs == 0)
        {
            if (current.Type == TokenType.Word)
            {
                _hasSeenWord = true;
            }
            return current;
        }

        // Case 2: one EOF
        // skip, continue to nect token
        if (consecutiveEOLs == 1)
        {
            if (current.Type == TokenType.Word)
            {
                _hasSeenWord = true;
            }
            return current;
        }

        // Case 3: more than 2 EOL
        // consecutiveEOLs >= 2
        if (!_hasSeenWord)
        {
            if (current.Type == TokenType.Word)
            {
                _hasSeenWord = true;
            }
            return current;
        }

        if (current.Type == TokenType.EndOfInput)
        {
            _bufferedToken = current;
            return new Token(TokenType.EndOfParagraph);
        }
        else
        {
            if (current.Type == TokenType.Word)
            {
                _hasSeenWord = true;
            }
            _bufferedToken = current;
            return new Token(TokenType.EndOfParagraph);
        }
    }
}