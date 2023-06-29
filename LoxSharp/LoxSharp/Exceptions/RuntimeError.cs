using LoxSharp.Models;

namespace LoxSharp.Exceptions;

public class RuntimeError : Exception
{
    public readonly Token token;

    public RuntimeError(Token _token, string message): base(message)
    {
        token = _token;
    }
}
