namespace InfrastSim.Script;
internal class TokenSequence(IList<Token> tokens) {
    IList<Token> tokens = tokens;
    int cur = 0;

    public Token MoveNext() {
        if (cur + 1 == tokens.Count) {
            return tokens[cur];
        }
        return tokens[++cur];
    }
    public Token Peek() {
        if (cur + 1 < tokens.Count) {
            return tokens[cur + 1];
        } else {
            return tokens.Last();
        }
    }
    public void Reset() => cur = 0;

    public Token Current => tokens[cur];
    public IList<Token> GetTokens() => tokens;
    public int Count => tokens.Count;
    public Token At(int index) => tokens[index];
}
