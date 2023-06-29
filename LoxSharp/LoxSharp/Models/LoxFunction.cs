using LoxSharp.Exceptions;
using LoxSharp.Interfaces;

namespace LoxSharp.Models;

public class LoxFunction : ILoxCallable
{
    private readonly Stmt.Function declaration;
    private readonly LoxEnvironment closure;

    public LoxFunction(Stmt.Function _declaration, LoxEnvironment _closure)
    {
        declaration = _declaration;
        closure = _closure;
    }

    public LoxFunction bind(LoxInstance instance)
    {
        LoxEnvironment environment = new(closure);
        environment.define("this", instance);

        return new LoxFunction(declaration, environment);
    }

    public int arity()
    {
        return declaration.parameters.Count;
    }

    public object call(Interpreter interpreter, List<object> arguments)
    {
        LoxEnvironment environment = new(closure);
        for (int i = 0; i < declaration.parameters.Count; i++)
        {
            environment.define(declaration.parameters[i].lexeme, arguments[i]);
        }

        try
        {
            interpreter.executeBlock(declaration.body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn {declaration.name.lexeme}>";
    }
}
