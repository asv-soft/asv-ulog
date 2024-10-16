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

public class ULogSizeTooSmallException(string section)
    : ULogException($"Size too small to read {section}");
    
public sealed class WrongTokenSectionException() 
    : ULogException("Token was found in a wrong section");

public sealed class UnknownTokenException() 
    : ULogException("Unknown token was found");