namespace InfrastSim.Script;
internal class Scanner {
    string inputs;
    int index = 0;
    int line = 0;
    int col = 0;
    int forwardSlashCount = 0;
    List<Token> tokens = new();

    private Scanner(string inputs) {
        this.inputs = inputs;
    }
    private void ScanImpl() {
        tokens.Add(new Token(TokenType.Begin, string.Empty, 0, 0));
        while (index < inputs.Length) {
            ReadToken();
        }
        tokens.Add(new Token(TokenType.Final, string.Empty, line + 1, 0));
    }

    private char CurrentChar => inputs[index];
    private bool ValidStringChar(char ch)
        => !(char.IsWhiteSpace(ch) || @"/\#;,.()*|[]!?@$%&+=<>`~'""".Contains(ch));
    private void MoveNext(int n = 1) { index += n; col += n; }
    private void NewLine() { index++; line++; col = 0; forwardSlashCount = 0; }

    private void ReadToken() {
        int tokenIndex = index;
        int tokenLength = 1;
        TokenType tokenType = TokenType.None;
        switch (CurrentChar) {
            case '/':
                tokenType = TokenType.ForwardSlash;
                break;
            case '#':
                tokenType = TokenType.NumberSign;
                break;
            case ';':
                tokenType = TokenType.Semicolon;
                break;
            case '\n':
                tokenType = TokenType.NewLine;
                break;
            default:
                if (ValidStringChar(CurrentChar)) {
                    tokenType = TokenType.String;
                    while (tokenIndex + tokenLength < inputs.Length
                        && ValidStringChar(inputs[tokenIndex + tokenLength])) {
                        tokenLength++;
                    }
                } else if (!char.IsWhiteSpace(CurrentChar)) {
                    tokenType = TokenType.UnknownMark;
                } else {
                    // whitespaces
                }
                break;
        }

        if (tokenType == TokenType.ForwardSlash) {
            forwardSlashCount++;
            if (forwardSlashCount == 2) {
                tokens.RemoveAt(tokens.Count - 1);
                tokenIndex -= 1;
                tokenLength = 2;
                tokenType = TokenType.Comment;
                while (tokenIndex + tokenLength < inputs.Length
                    && inputs[tokenIndex + tokenLength] != '\n') {
                    tokenLength++;
                }
            }
        } else if (tokenType == TokenType.NumberSign) {
            tokenType = TokenType.Comment;
            while (tokenIndex + tokenLength < inputs.Length
                && inputs[tokenIndex + tokenLength] != '\n') {
                tokenLength++;
            }
        } else {
            forwardSlashCount = 0;
        }

        if (tokenType != TokenType.None) {
            var token = new Token(
                tokenType, inputs.Substring(tokenIndex, tokenLength), line, col);
            tokens.Add(token);
        }

        if (tokenType == TokenType.NewLine) {
            NewLine();
        } else {
            MoveNext(tokenLength);
        }
    }

    static public TokenSequence Scan(string inputs) {
        var scanner = new Scanner(inputs);
        scanner.ScanImpl();
        return new(scanner.tokens);
    }
}
