using LoxSharp.Exceptions;
using LoxSharp.Models;

namespace LoxSharp;

public class LoxEnvironment
{
    public readonly LoxEnvironment? enclosing;
    private readonly Dictionary<string, object> values = new();

    public LoxEnvironment()
    {
        enclosing = null;
    }

    public LoxEnvironment(LoxEnvironment _enclosing)
    {
        enclosing = _enclosing;
    }

    public void define(string name, object value)
    {
        values[name] = value;
    }

    public LoxEnvironment ancestor(int distance)
    {
        LoxEnvironment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.enclosing;
        }

        return environment;
    }

    public object get(Token name)
    {
        if (values.ContainsKey(name.lexeme))
        {
            return values[name.lexeme];
        }

        if (enclosing != null) return enclosing.get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public object getAt(int distance, string name)
    {
        return ancestor(distance).values.GetValueOrDefault(name);
    }

    public void assign(Token name, object value)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values[name.lexeme] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public void assignAt(int distance, Token name, object value)
    {
        ancestor(distance).values.Add(name.lexeme, value);
    }
}
