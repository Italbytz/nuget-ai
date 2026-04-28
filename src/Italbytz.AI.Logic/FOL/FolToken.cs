namespace Italbytz.AI.Logic.Fol;

public enum FolTokenType
{
    Identifier,
    LeftParen,
    RightParen,
    Comma,
    Equals,
    Not,
    And,
    Or,
    Implies,
    ForAll,
    Exists,
    End
}

public sealed record FolToken(FolTokenType Type, string Lexeme);