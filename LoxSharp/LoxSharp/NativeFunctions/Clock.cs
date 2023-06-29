using LoxSharp.Interfaces;

namespace LoxSharp.NativeFunctions;

public class Clock : ILoxCallable
{
    public static readonly Clock Instance = new Clock();

    public int arity()
    {
        return 0;
    }

    public object call(Interpreter interpreter, List<object> arguments)
    {
        return (double)DateTime.Now.Second;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}
