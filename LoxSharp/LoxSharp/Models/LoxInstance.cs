using LoxSharp.Exceptions;

namespace LoxSharp.Models;

public class LoxInstance
{
    private LoxClass klass;
    private readonly Dictionary<string, object> fields = new();

    public LoxInstance(LoxClass _klass)
    {
        klass = _klass;
    }

    public object get(Token name)
    {
        if (fields.ContainsKey(name.lexeme))
        {
            return fields[name.lexeme];
        }

        LoxFunction method = klass.findMethod(name.lexeme);
        if (method != null) return method.bind(this);

        throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
    }

    public void set(Token name, object value)
    {
        fields.Add(name.lexeme, value);
    }

    public override string ToString()
    {
        return klass.name + " instance";
    }
}
