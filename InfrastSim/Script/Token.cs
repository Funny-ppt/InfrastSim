namespace InfrastSim.Script;
internal record Token(TokenType Type, string RawValue, int Line, int Column) {
    public override string ToString() {
        return $"{RawValue}<{Type}>[ln {Line + 1}, col {Column + 1}]";
    }
}
