using LoxSharp.Enums;
using LoxSharp.Exceptions;
using LoxSharp.Models;
using LoxSharp.Utils;

namespace LoxSharp;

internal class Program
{
    private static readonly Interpreter interpreter = new();
    static bool hadError = false;
    static bool hadRuntimeError = false;

    static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: LoxSharp [script]");
            Environment.Exit(1);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            //Expr expression = new Expr.Binary(
                //new Expr.Unary(new Token(TokenType.Minus, "-", null, 1), new Expr.Literal(123)),
                //new Token(TokenType.Star, "*", null, 1),
                //new Expr.Grouping(new Expr.Literal(45.67)));

            //Console.WriteLine(new AstPrinter().print(expression));
            RunPrompt();
        }
    }

    private static void RunFile(string path)
    {
        string text = File.ReadAllText(path);
        Run(text);

        if (hadError)
            Environment.Exit(1);

        if (hadRuntimeError)
            Environment.Exit(1);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.WriteLine("> ");
            string line = Console.ReadLine();
            if (line == null)
                break;

            Run(line);

            hadError = false;
        }
    }

    private static void Run(string source)
    {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.ScanTokens();
        
        Parser parser = new(tokens);
        List<Stmt> statements = parser.parse();

        // Stop if there was a syntax error
        if (hadError) return;

        Resolver resolver = new(interpreter);
        resolver.resolve(statements);

        // Stop if there was a resolution error
        if (hadError) return;

        interpreter.interpret(statements);
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        hadError = true;
    }

    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
            Report(token.line, " at end", message);
        else
            Report(token.line, " at '" + token.lexeme + "'", message);
    }

    public static void runtimeError(RuntimeError error)
    {
        Console.Error.WriteLine(error.Message + "\n[line " + error.token.line + "]");
        hadRuntimeError = true;
    }
}