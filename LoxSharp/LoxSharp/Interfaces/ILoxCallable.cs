namespace LoxSharp.Interfaces;

public interface ILoxCallable
{
    int arity();
    object call(Interpreter interpreter, List<object> arguments);
}
