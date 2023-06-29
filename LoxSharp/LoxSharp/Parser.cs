using LoxSharp.Enums;
using LoxSharp.Exceptions;
using LoxSharp.Models;

namespace LoxSharp;

public class Parser
{

    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> _tokens)
    {
        tokens = _tokens;
    }

    public List<Stmt> parse()
    {
        List<Stmt> statements = new();

        while (!isAtEnd())
        {
            statements.Add(declaration());
        }

        return statements;
    }

    #region Statements
    private Stmt declaration()
    {
        try
        {
            if (match(TokenType.Class)) return classDeclaration();
            if (match(TokenType.Fun)) return function("function");
            if (match(TokenType.Var)) return varDeclaration();

            return statement();
        }
        catch (ParseError error)
        {
            synchronize();
            return null;
        }
    }

    private Stmt classDeclaration()
    {
        Token name = consume(TokenType.Identifier, "Expect class name.");
        consume(TokenType.LeftBrace, "Expect '{' before class body.");

        List<Stmt.Function> methods = new();
        while (!check(TokenType.RightBrace) && !isAtEnd())
        {
            methods.Add(function("method"));
        }

        consume(TokenType.RightBrace, "Expect '}' after class body.");

        return new Stmt.Class(name, methods);
    }

    private Stmt varDeclaration()
    {
        Token name = consume(TokenType.Identifier, "Expect variable name.");

        Expr? initializer = null;
        if (match(TokenType.Equal))
        {
            initializer = expression();
        }

        consume(TokenType.SemiColon, "Expect ';' after variable declaration");
        return new Stmt.Var(name, initializer);
    }

    private Stmt statement()
    {
        if (match(TokenType.For)) return forStatement();
        if (match(TokenType.If)) return ifStatement();
        if (match(TokenType.Print)) return printStatment();
        if (match(TokenType.Return)) return returnStatement();
        if (match(TokenType.While)) return whileStatement();
        if (match(TokenType.LeftBrace)) return new Stmt.Block(block());

        return expressionStatement();
    }

    private Stmt forStatement()
    {
        consume(TokenType.LeftParen, "Expect '(' after 'for'");

        Stmt? initializer;
        if (match(TokenType.SemiColon))
        {
            initializer = null;
        }
        else if (match(TokenType.Var))
        {
            initializer = varDeclaration();
        }
        else
        {
            initializer = expressionStatement();
        }

        Expr? condition = null;
        if (!check(TokenType.SemiColon))
        {
            condition = expression();
        }

        consume(TokenType.SemiColon, "Expect ';' after loop condition");

        Expr? increment = null;
        if (!check(TokenType.RightParen))
        {
            increment = expression();
        }

        consume(TokenType.RightParen, "Expect ')' after for clauses");

        Stmt body = statement();

        if (increment != null)
        {
            body = new Stmt.Block(new List<Stmt>()
            {
                body,
                new Stmt.Expression(increment)
            });
        }

        if (condition == null)
        {
            condition = new Expr.Literal(true);
        }

        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block(new List<Stmt>() { initializer, body });
        }

        return body;
    }

    private Stmt whileStatement()
    {
        consume(TokenType.LeftParen, "Expect '(' after 'while'");
        
        Expr condition = expression();

        consume(TokenType.RightParen, "Expect ')' after condition");

        Stmt body = statement();

        return new Stmt.While(condition, body);
    }

    private Stmt ifStatement()
    {
        consume(TokenType.LeftParen, "Expect '(' after 'if'.");

        Expr condition = expression();

        consume(TokenType.RightParen, "Expect ')' after if condition.");

        Stmt thenBranch = statement();
        Stmt? elseBranch = null;
        
        if (match(TokenType.Else))
        {
            elseBranch = statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt printStatment()
    {
        Expr value = expression();
        consume(TokenType.SemiColon, "Expect ';' after value");
        return new Stmt.Print(value);
    }

    private Stmt returnStatement()
    {
        Token keyword = previous();
        Expr? value = null;

        if (!check(TokenType.SemiColon))
        {
            value = expression();
        }

        consume(TokenType.SemiColon, "Expect ';' after return value");
        return new Stmt.Return(keyword, value);
    }

    private Stmt expressionStatement()
    {
        Expr expr = expression();
        consume(TokenType.SemiColon, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Stmt.Function function(string kind)
    {
        Token name = consume(TokenType.Identifier, $"Expect {kind} name.");

        consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");

        List<Token> parameters = new();
        if (!check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    error(peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(consume(TokenType.Identifier, "Expect parameter name."));
            } while (match(TokenType.Comma));
        }

        consume(TokenType.RightParen, "Expect ')' after parameters.");

        consume(TokenType.LeftBrace, "Expect '{' before " + kind + " body.");

        List<Stmt> body = block();

        return new Stmt.Function(name, parameters, body);
    }

    private List<Stmt> block()
    {
        List<Stmt> statements = new();

        while (!check(TokenType.RightBrace) && !isAtEnd())
        {
            statements.Add(declaration());
        }

        consume(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }
    #endregion

    #region Expressions
    private Expr expression()
    {
        return assignment();
    }

    private Expr assignment()
    {
        Expr expr = or();

        if (match(TokenType.Equal))
        {
            Token equals = previous();
            Expr value = assignment();

            if (expr is Expr.Variable)
            {
                Token name = ((Expr.Variable)expr).name;
                return new Expr.Assign(name, value);
            }
            else if (expr is Expr.Get)
            {
                Expr.Get get = (Expr.Get)expr;
                return new Expr.Set(get.obj, get.name, value);
            }

            error(equals, "Invalid assignment target");
        }

        return expr;
    }

    private Expr or()
    {
        Expr expr = and();

        while (match(TokenType.Or))
        {
            Token oper = previous();
            Expr right = and();
            
            expr = new Expr.Logical(expr, oper, right);
        }

        return expr;
    }

    private Expr and()
    {
        Expr expr = equality();

        while (match(TokenType.And))
        {
            Token oper = previous();
            Expr right = equality();

            expr = new Expr.Logical(expr, oper, right);
        }

        return expr;
    }

    private Expr equality()
    {
        Expr expr = comparison();

        while (match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            Token oper = previous();
            Expr right = comparison();

            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr comparison()
    {
        Expr expr = term();

        while (match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            Token oper = previous();
            Expr right = term();

            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr term()
    {
        Expr expr = factor();

        while (match(TokenType.Minus, TokenType.Plus))
        {
            Token oper = previous();
            Expr right = factor();

            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr factor()
    {
        Expr expr = unary();

        while (match(TokenType.Slash, TokenType.Star))
        {
            Token oper = previous();
            Expr right = unary();

            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr unary()
    {
        if (match(TokenType.Bang, TokenType.Minus))
        {
            Token oper = previous();
            Expr right = unary();

            return new Expr.Unary(oper, right);
        }

        return call();
    }

    private Expr call()
    {
        Expr expr = primary();

        while (true)
        {
            if (match(TokenType.LeftParen))
            {
                expr = finishCall(expr);
            }
            else if (match(TokenType.Dot))
            {
                Token name = consume(TokenType.Identifier, "Expect property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr primary()
    {
        if (match(TokenType.False)) return new Expr.Literal(false);
        if (match(TokenType.True)) return new Expr.Literal(true);
        if (match(TokenType.Nil)) return new Expr.Literal(null);
        if (match(TokenType.Number, TokenType.String)) return new Expr.Literal(previous().literal);

        if (match(TokenType.This)) return new Expr.This(previous());

        if (match(TokenType.Identifier)) return new Expr.Variable(previous());

        if (match(TokenType.LeftParen))
        {
            Expr expr = expression();
            consume(TokenType.RightParen, "Expect ')' after expression");
            return new Expr.Grouping(expr);
        }

        throw error(peek(), "Expect expression.");
    }
    #endregion

    #region Helper Methods
    private bool match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (check(type))
            {
                advance();
                return true;
            }
        }

        return false;
    }

    private bool check(TokenType type)
    {
        if (isAtEnd()) return false;
        return peek().type == type;
    }

    private Token advance()
    {
        if (!isAtEnd()) current++;
        return previous();
    }

    private bool isAtEnd()
    {
        return peek().type == TokenType.EOF;
    }

    private Token peek()
    {
        return tokens[current];
    }

    private Token previous()
    {
        return tokens[current - 1];
    }

    private Token consume(TokenType type, string message)
    {
        if (check(type)) return advance();

        throw error(peek(), message);
    }

    private ParseError error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseError();
    }

    private void synchronize()
    {
        advance();

        while (!isAtEnd())
        {
            if (previous().type == TokenType.SemiColon) return;

            switch (peek().type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            advance();
        }
    }

    private Expr finishCall(Expr callee)
    {
        List<Expr> arguments = new();
        if (!check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    error(peek(), "Can't have more than 255 arguments");
                }
                arguments.Add(expression());
            } while (match(TokenType.Comma));
        }

        Token paren = consume(TokenType.RightParen, "Expect ')' after arguments");

        return new Expr.Call(callee, paren, arguments);
    }
    #endregion
}
