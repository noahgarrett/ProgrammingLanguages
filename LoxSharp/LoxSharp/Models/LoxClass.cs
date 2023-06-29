using LoxSharp.Interfaces;

namespace LoxSharp.Models;

public class LoxClass : ILoxCallable
{
    public readonly string name;
    private readonly Dictionary<string, LoxFunction> methods;

    public LoxClass(string _name, Dictionary<string, LoxFunction> _methods)
    {
        name = _name;
        methods = _methods;
    }

    public LoxFunction findMethod(string name)
    {
        if (methods.ContainsKey(name))
        {
            return methods[name];
        }

        return null;
    }

    public int arity()
    {
        return 0;
    }

    public object call(Interpreter interpreter, List<object> arguments)
    {
        LoxInstance instance = new(this);
        return instance;
    }

    public override string ToString()
    {
        return name;
    }
}
