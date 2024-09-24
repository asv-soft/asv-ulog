namespace Asv.ULog.ULogFile.Processors;

public sealed class LoggedStringMessage
{
    public required ULogLoggedStringMessageToken.ULogLevel LogLevel { get; init; }
    
    public required TimeSpan Time { get; init; }
    
    public required string Message { get; init; }
}