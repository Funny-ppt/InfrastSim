namespace InfrastSim.Script;
internal class Parser(TokenSequence tokens) {
    private TokenSequence tokens = tokens;

    public static Script Parse(TokenSequence tokens) {
        return new Parser(tokens).ParseScript();
    }

    private bool Match(TokenType type) {
        if (tokens.Current.Type == type) {
            tokens.MoveNext();
            return true;
        }
        return false;
    }

    private Script ParseScript() {
        tokens.Reset();

        if (tokens.Current.Type == TokenType.Begin) {
            tokens.MoveNext();
        }

        var statements = new List<Statement>();
        while (tokens.Current.Type != TokenType.Final) {
            statements.Add(ParseStatement());
        }
        return new(statements);
    }

    private Statement ParseStatement() {
        while (tokens.Current.Type == TokenType.NewLine || tokens.Current.Type == TokenType.Comment) {
            tokens.MoveNext();
        }

        var command = ParseCommand();
        var parameters = new List<Parameter>();
        while (tokens.Current.Type == TokenType.String) {
            parameters.Add(ParseParameter());
        }
        Match(TokenType.Semicolon); // Optional semicolon
        return new Statement(command, parameters);
    }

    private Command ParseCommand() {
        if (tokens.Current.Type != TokenType.String) {
            throw new ParseException("Expected a command.", tokens.Current.Line, tokens.Current.Column);
        }
        var command = new Command(tokens.Current.RawValue);
        tokens.MoveNext();
        return command;
    }

    private Parameter ParseParameter() {
        if (tokens.Current.Type != TokenType.String) {
            throw new ParseException("Expected a parameter.", tokens.Current.Line, tokens.Current.Column);
        }
        var parameter = new Parameter(tokens.Current.RawValue);
        tokens.MoveNext();
        return parameter;
    }
}

internal class Script(List<Statement> statements) {
    public List<Statement> Statements { get; } = statements;
}

internal class Statement(Command command, List<Parameter> parameters) {
    public Command Command { get; } = command;
    public List<Parameter> Parameters { get; } = parameters;

    public override string ToString() {
        return $"{Command.Value} [{string.Join(',', Parameters.Select(p => p.Value))}]";
    }
}

internal class Command(string value) {
    public string Value { get; } = value;
}

internal class Parameter(string value) {
    public string Value { get; } = value;
}

internal class ParseException(string message, int line, int col)
    : Exception($"Parse error at line {line + 1}, column {col + 1}: {message}")
{
}
