namespace Asv.ULog;

public class ULogWriterException : Exception
{
    public ULogWriterException()
    {
    }

    public ULogWriterException(string message) : base(message)
    {
    }

    public ULogWriterException(string message, Exception inner) : base(message, inner)
    {
    }
}