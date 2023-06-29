using LoxSharp.Enums;

namespace LoxSharp.Models;

public class Token
{
    public readonly TokenType type;
    public readonly string lexeme;
    public readonly object literal;
    public readonly int line;

    public Token(TokenType Type, string Lexeme, object Literal, int Line)
    {
        this.type = Type;
        this.lexeme = Lexeme;
        this.literal = Literal;
        this.line = Line;
    }

    public string toString()
    {
        return $"[{type} {lexeme} {literal}]";
    }
}
