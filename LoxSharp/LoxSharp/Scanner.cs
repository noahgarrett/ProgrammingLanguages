using LoxSharp.Enums;
using LoxSharp.Models;

namespace LoxSharp;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = new();
    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        { "and", TokenType.And },
        { "class", TokenType.Class },
        { "else", TokenType.Else },
        { "false", TokenType.False },
        { "for", TokenType.For },
        { "fun", TokenType.Fun },
        { "if", TokenType.If },
        { "nil", TokenType.Nil },
        { "or", TokenType.Or },
        { "print", TokenType.Print },
        { "return", TokenType.Return },
        { "super", TokenType.Super },
        { "this", TokenType.This },
        { "true", TokenType.True },
        { "var", TokenType.Var },
        { "while", TokenType.While }
    };

    public Scanner(string _source)
    {
        source = _source;
    }

    public List<Token> ScanTokens()
    {
        while (!isAtEnd())
        {
            // We are at the beginning of the next lexeme.
            start = current;
            scanToken();
        }

        tokens.Add(new Token(TokenType.EOF, "", null, line));
        return tokens;
    }

    private void scanToken()
    {
        char c = advance();
        switch (c)
        {
            case '(': 
                addToken(TokenType.LeftParen); break;
            case ')':
                addToken(TokenType.RightParen); break;
            case '{':
                addToken(TokenType.LeftBrace); break;
            case '}':
                addToken(TokenType.RightBrace); break;
            case ',':
                addToken(TokenType.Comma); break;
            case '.':
                addToken(TokenType.Dot); break;
            case '-':
                addToken(TokenType.Minus); break;
            case '+':
                addToken(TokenType.Plus); break;
            case ';':
                addToken(TokenType.SemiColon); break;
            case '*':
                addToken(TokenType.Star); break;
            case '!':
                addToken(match('=') ? TokenType.BangEqual : TokenType.Bang); break;
            case '=':
                addToken(match('=') ? TokenType.EqualEqual : TokenType.Equal); break;
            case '<':
                addToken(match('=') ? TokenType.LessEqual : TokenType.Less); break;
            case '>':
                addToken(match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
            case '/':
                if (match('/'))
                {
                    while (peek() != '\n' && !isAtEnd())
                        advance();
                }
                else
                {
                    addToken(TokenType.Slash);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace
                break;
            case '\n':
                line++;
                break;
            case '"':
                makeString(); break;
            default:
                if (isDigit(c))
                {
                    makeNumber();
                }
                else if (isAlpha(c))
                {
                    makeIdentifier();
                }
                else
                {
                    Program.Error(line, "Unexpected Character.");
                }
                break;
        }
    }

    private void makeIdentifier()
    {
        while (isAlphaNumeric(peek())) advance();

        string text = source.Substring(start, current - start);

        TokenType type = TokenType.Identifier;
        if (keywords.ContainsKey(text)) 
            type = keywords[text];

        addToken(type);
    }

    private void makeNumber()
    {
        while (isDigit(peek())) advance();

        // Look for a fractional part
        if (peek() == '.' && isDigit(peekNext()))
        {
            advance();

            while (isDigit(peek())) advance();
        }

        addToken(TokenType.Number, Convert.ToDouble(source.Substring(start, current - start)));
    }

    private void makeString()
    {
        while (peek() != '"' && !isAtEnd())
        {
            if (peek() == '\n') line++;
            advance();
        }

        if (isAtEnd())
        {
            Program.Error(line, "Unterminated string.");
            return;
        }

        advance();

        // Trim the surrounding quotes
        // If we support escape characters, those would be handled here.
        string value = source.Substring(start + 1, (current - 2) - start);
        addToken(TokenType.String, value);
    }

    private bool match(char expected)
    {
        if (isAtEnd()) return false;
        if (source[current] != expected) return false;

        current++;
        return true;
    }

    private char peek()
    {
        if (isAtEnd()) return '\0';
        return source[current];
    }

    private char peekNext()
    {
        if (current + 1 >= source.Length) return '\0';
        return source[current + 1];
    }

    private bool isAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    }

    private bool isAlphaNumeric(char c)
    {
        return isAlpha(c) || isDigit(c);
    }

    private bool isDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool isAtEnd()
    {
        return current >= source.Length;
    }

    private char advance()
    {
        current++;
        return source[current - 1];
    }

    private void addToken(TokenType type)
    {
        addToken(type, null);
    }

    private void addToken(TokenType type, object literal)
    {
        string text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
}
