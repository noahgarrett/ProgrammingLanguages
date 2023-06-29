namespace LoxSharp.Exceptions;

public class Return : Exception
{
    public readonly object value;

    public Return(object _value)
    {
        value = _value;
    }
}
