namespace InfrastSim.Script;
internal enum TokenType {
    None,
    Semicolon,          // ;
    ForwardSlash,       // /
    NumberSign,         // #
    UnknownMark,
    NewLine,
    String,
    Comment,
    Begin,
    Final,
}
