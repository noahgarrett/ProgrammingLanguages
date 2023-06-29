using LoxSharp.Models;
using LoxSharp.Utils;

namespace LoxSharp;

public class Resolver : Expr.Visitor<Unit>, Stmt.Visitor<Unit>
{
    private readonly Interpreter interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private FunctionType currentFunction = FunctionType.NONE;

    public Resolver(Interpreter _interpreter)
    {
        interpreter = _interpreter;
    }

    private enum FunctionType
    {
        NONE,
        FUNCTION,
        METHOD
    }

    #region Expression Nodes
    public Unit visitAssignExpr(Expr.Assign expr)
    {
        resolve(expr.value);
        resolveLocal(expr, expr.name);
        return Unit.Default;
    }

    public Unit visitBinaryExpr(Expr.Binary expr)
    {
        resolve(expr.left);
        resolve(expr.right);

        return Unit.Default;
    }

    public Unit visitCallExpr(Expr.Call expr)
    {
        resolve(expr.callee);

        foreach (Expr argument in expr.arguments) resolve(argument);

        return Unit.Default;
    }

    public Unit visitGetExpr(Expr.Get expr)
    {
        resolve(expr.obj);
        return Unit.Default;
    }

    public Unit visitSetExpr(Expr.Set expr)
    {
        resolve(expr.value);
        resolve(expr.obj);
        return Unit.Default;
    }

    public Unit visitThisExpr(Expr.This expr)
    {
        resolveLocal(expr, expr.keyword);
        return Unit.Default;
    }

    public Unit visitGroupingExpr(Expr.Grouping expr)
    {
        resolve(expr.expression);
        return Unit.Default;
    }

    public Unit visitLiteralExpr(Expr.Literal expr)
    {
        return Unit.Default;
    }

    public Unit visitLogicalExpr(Expr.Logical expr)
    {
        resolve(expr.left);
        resolve(expr.right);
        return Unit.Default;
    }

    public Unit visitUnaryExpr(Expr.Unary expr)
    {
        resolve(expr.right);
        return Unit.Default;
    }

    public Unit visitVariableExpr(Expr.Variable expr)
    {
        if (scopes.Count > 0 && scopes.Peek().GetValueOrDefault(expr.name.lexeme) == false)
        {
            Program.Error(expr.name, "Can't read local variable in its own initializer.");
        }

        resolveLocal(expr, expr.name);
        return Unit.Default;
    }
    #endregion

    #region Statement Nodes
    public Unit visitBlockStmt(Stmt.Block stmt)
    {
        beginScope();
        resolve(stmt.statements);
        endScope();

        return Unit.Default;
    }

    public Unit visitClassStmt(Stmt.Class stmt)
    {
        declare(stmt.name);
        define(stmt.name);

        beginScope();
        scopes.Peek().Add("this", true);

        foreach (Stmt.Function method in stmt.methods)
        {
            FunctionType declaration = FunctionType.METHOD;
            resolveFunction(method, declaration);
        }

        endScope();

        return Unit.Default;
    }

    public Unit visitExpressionStmt(Stmt.Expression stmt)
    {
        resolve(stmt.expression); 
        return Unit.Default;
    }

    public Unit visitFunctionStmt(Stmt.Function stmt)
    {
        declare(stmt.name);
        define(stmt.name);

        resolveFunction(stmt, FunctionType.FUNCTION);

        return Unit.Default;
    }

    public Unit visitIfStmt(Stmt.If stmt)
    {
        resolve(stmt.condition);
        resolve(stmt.thenBranch);
        if (stmt.elseBranch != null) resolve(stmt.elseBranch);

        return Unit.Default;
    }

    public Unit visitPrintStmt(Stmt.Print stmt)
    {
        resolve(stmt.expression);
        return Unit.Default;
    }

    public Unit visitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == FunctionType.NONE)
        {
            Program.Error(stmt.keyword, "Can't return from top-level code");
        }

        if (stmt.value != null) resolve(stmt.value);

        return Unit.Default;
    }

    public Unit visitVarStmt(Stmt.Var stmt)
    {
        declare(stmt.name);

        if (stmt.intitializer != null)
            resolve(stmt.intitializer);

        define(stmt.name);

        return Unit.Default;
    }

    public Unit visitWhileStmt(Stmt.While stmt)
    {
        resolve(stmt.condition);
        resolve(stmt.body);

        return Unit.Default;
    }
    #endregion

    #region Helpers
    public void resolve(List<Stmt> statements)
    {
        foreach (Stmt stmt in statements)
        {
            resolve(stmt);
        }
    }

    private void resolve(Stmt stmt)
    {
        stmt.accept(this);
    }

    private void resolve(Expr expr)
    {
        expr.accept(this);
    }

    private void beginScope()
    {
        scopes.Push(new Dictionary<string, bool>());
    }

    private void endScope()
    {
        scopes.Pop();
    }

    private void declare(Token name)
    {
        if (scopes.Count == 0) return;

        Dictionary<string, bool> scope = scopes.Peek();
        if (scope.ContainsKey(name.lexeme))
        {
            Program.Error(name, "Already a variable with this name in this scope");
        }

        scope.Add(name.lexeme, false);
    }

    private void define(Token name)
    {
        if (scopes.Count == 0) return;

        scopes.Peek().Add(name.lexeme, true);
    }

    private void resolveLocal(Expr expr, Token name)
    {
        for (int i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes.ElementAt(i).ContainsKey(name.lexeme))
            {
                interpreter.resolve(expr, scopes.Count - 1 - i);
                return;
            }
        }
    }

    private void resolveFunction(Stmt.Function function, FunctionType type)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = type;

        beginScope();

        foreach (Token param in function.parameters)
        {
            declare(param);
            define(param);
        }

        resolve(function.body);
        endScope();

        currentFunction = enclosingFunction;
    }
    #endregion
}
