using LoxSharp.Enums;
using LoxSharp.Exceptions;
using LoxSharp.Interfaces;
using LoxSharp.Models;
using LoxSharp.NativeFunctions;
using LoxSharp.Utils;

namespace LoxSharp;

public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<Unit>
{
    public static readonly LoxEnvironment globals = new();
    private readonly Dictionary<Expr, int> locals = new();
    private LoxEnvironment environment = globals;

    public Interpreter()
    {
        globals.define("clock", Clock.Instance);
    }

    public LoxEnvironment GetGlobalEnv()
    {
        return globals;
    }

    public void interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
                execute(statement);
        }
        catch (RuntimeError error)
        {
            Program.runtimeError(error);
        }
    }

    private void execute(Stmt stmt)
    {
        stmt.accept(this);
    }

    public void resolve(Expr expr, int depth)
    {
        locals.Add(expr, depth);
    }

    public void executeBlock(List<Stmt> statements, LoxEnvironment env)
    {
        LoxEnvironment previous = environment;

        try
        {
            environment = env;

            foreach (Stmt statement in statements)
            {
                execute(statement);
            }
        }
        finally
        {
            environment = previous;
        }
    }

    private object evaluate(Expr expr)
    {
        return expr.accept(this);
    }

    #region Statement Nodes
    Unit Stmt.Visitor<Unit>.visitPrintStmt(Stmt.Print stmt)
    {
        object value = evaluate(stmt.expression);
        Console.WriteLine(stringify(value));
        return Unit.Default;
    }

    Unit Stmt.Visitor<Unit>.visitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.value != null)
            value = evaluate(stmt.value);

        throw new Return(value);
    }

    Unit Stmt.Visitor<Unit>.visitExpressionStmt(Stmt.Expression stmt)
    {
        evaluate(stmt.expression);
        return Unit.Default;
    }

    Unit Stmt.Visitor<Unit>.visitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.intitializer != null)
        {
            value = evaluate(stmt.intitializer);
        }

        environment.define(stmt.name.lexeme, value);
        return Unit.Default;
    }

    Unit Stmt.Visitor<Unit>.visitBlockStmt(Stmt.Block stmt)
    {
        executeBlock(stmt.statements, new LoxEnvironment(environment));

        return Unit.Default;
    }

    Unit Stmt.Visitor<Unit>.visitClassStmt(Stmt.Class stmt)
    {
        environment.define(stmt.name.lexeme, null);

        Dictionary<string, LoxFunction> methods = new();
        foreach (Stmt.Function method in stmt.methods)
        {
            LoxFunction function = new(method, environment);
            methods.Add(method.name.lexeme, function);
        }

        LoxClass klass = new(stmt.name.lexeme, methods);
        environment.assign(stmt.name, klass);

        return Unit.Default;
    }


   Unit Stmt.Visitor<Unit>.visitIfStmt(Stmt.If stmt)
    {
        if (isTruthy(evaluate(stmt.condition)))
        {
            execute(stmt.thenBranch);
        }
        else if (stmt.elseBranch != null)
        {
            execute(stmt.elseBranch);
        }

        return Unit.Default;
    }

    Unit Stmt.Visitor<Unit>.visitWhileStmt(Stmt.While stmt)
    {
        while (isTruthy(evaluate(stmt.condition))) 
        {
            execute(stmt.body);
        }

        return Unit.Default;
    }

    Unit Stmt.Visitor<Unit>.visitFunctionStmt(Stmt.Function stmt)
    {
        LoxFunction function = new(stmt, environment);
        environment.define(stmt.name.lexeme, function);
        return Unit.Default;
    }
    #endregion

    #region Expression Nodes
    public object visitBinaryExpr(Expr.Binary expr)
    {
        object left = evaluate(expr.left);
        object right = evaluate(expr.right);

        switch (expr.oper.type)
        {
            case TokenType.Greater:
                checkNumberOperands(expr.oper, left, right);
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                checkNumberOperands(expr.oper, left, right);
                return (double)left >= (double)right;
            case TokenType.Less:
                checkNumberOperands(expr.oper, left, right);
                return (double)left < (double)right;
            case TokenType.LessEqual:
                checkNumberOperands(expr.oper, left, right);
                return (double)left <= (double)right;
            case TokenType.BangEqual:
                return !isEqual(left, right);
            case TokenType.EqualEqual:
                return isEqual(left, right);
            case TokenType.Minus:
                checkNumberOperands(expr.oper, left, right);
                return (double)left - (double)right;
            case TokenType.Plus:
                if (left is double && right is double) return (double)left + (double)right;
                if (left is string && right is string) return (string)left + (string)right;

                throw new RuntimeError(expr.oper, "Operands must be two numbers or two strings");
            case TokenType.Slash:
                checkNumberOperands(expr.oper, left, right);
                return (double)left / (double)right;
            case TokenType.Star:
                checkNumberOperands(expr.oper, left, right);
                return (double)left * (double)right;
        }

        // Unreachable
        return null;
    }

    public object visitCallExpr(Expr.Call expr)
    {
        object callee = evaluate(expr.callee);

        List<object> arguments = new List<object>();
        foreach (Expr argument in expr.arguments)
        {
            arguments.Add(evaluate(argument));
        }

        if (callee is not ILoxCallable)
        {
            throw new RuntimeError(expr.paren, "Can only call functions and classes");
        }

        ILoxCallable function = (ILoxCallable)callee;
        if (arguments.Count != function.arity())
        {
            throw new RuntimeError(expr.paren, $"Expected {function.arity()} arguments but got {arguments.Count}.");
        }

        return function.call(this, arguments);
    }

    public object visitGetExpr(Expr.Get expr)
    {
        object obj = evaluate(expr.obj);
        if (obj is LoxInstance)
        {
            return ((LoxInstance)obj).get(expr.name);
        }

        throw new RuntimeError(expr.name, "Only instances have properties.");
    }

    public object visitSetExpr(Expr.Set expr)
    {
        object obj = evaluate(expr.obj);

        if (obj is not LoxInstance)
        {
            throw new RuntimeError(expr.name, "Only instances have fields");
        }

        object value = evaluate(expr.value);
        ((LoxInstance)obj).set(expr.name, value);
        return value;
    }

    public object visitThisExpr(Expr.This expr)
    {
        return lookupVariable(expr.keyword, expr);
    }

    public object visitGroupingExpr(Expr.Grouping expr)
    {
        return evaluate(expr.expression);
    }

    public object visitLiteralExpr(Expr.Literal expr)
    {
        return expr.value;
    }

    public object visitUnaryExpr(Expr.Unary expr)
    {
        object right = evaluate(expr.right);

        switch (expr.oper.type)
        {
            case TokenType.Bang:
                return !isTruthy(right);
            case TokenType.Minus:
                checkNumberOperand(expr.oper, right);
                return -(double)right;
        }

        // Unreachable
        return null;
    }

    public object visitVariableExpr(Expr.Variable expr)
    {
        return lookupVariable(expr.name, expr);
    }

    public object visitAssignExpr(Expr.Assign expr)
    {
        object value = evaluate(expr.value);

        var distance = locals.GetValueOrDefault(expr);
        if (distance != null)
        {
            environment.assignAt(distance, expr.name, value);
        }
        else
        {
            globals.assign(expr.name, value);
        }

        return value;
    }

    public object visitLogicalExpr(Expr.Logical expr)
    {
        object left = evaluate(expr.left);

        if (expr.oper.type == TokenType.Or)
        {
            if (isTruthy(left)) return left;
        }
        else
        {
            if (!isTruthy(left)) return left;
        }

        return evaluate(expr.right);
    }
    #endregion

    #region Helper Methods
    private bool isTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool) return (bool)obj;

        return true;
    }

    private bool isEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    private void checkNumberOperand(Token oper, object operand)
    {
        if (operand is double) return;

        throw new RuntimeError(oper, "Operand must be a number.");
    }

    private void checkNumberOperands(Token oper, object left, object right)
    {
        if (left is double && right is double) return;

        throw new RuntimeError(oper, "Operands must be numbers.");
    }

    private string stringify(object obj)
    {
        if (obj == null) return "nil";

        if (obj is double)
        {
            string text = obj.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }

        return obj.ToString();
    }

    private object lookupVariable(Token name, Expr expr)
    {
        var distance = locals.GetValueOrDefault(expr);
        if (distance != null)
        {
            return environment.getAt(distance, name.lexeme);
        }
        else
        {
            return globals.get(name);
        }
    }
    #endregion
}
