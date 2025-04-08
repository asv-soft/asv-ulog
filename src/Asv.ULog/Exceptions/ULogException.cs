namespace Asv.ULog;

public class ULogException : Exception
{
    public ULogException()
    {
    }

    public ULogException(string message) : base(message)
    {
    }

    public ULogException(string message, Exception inner) : base(message, inner)
    {
    }
}