namespace Asv.ULog.ULogFile.Processors;

public sealed class TaggedLoggedStringMessage
{
    public required ULogTaggedLoggedStringMessageToken.ULogLevel LogLevel { get; init; }

    public required TimeSpan Time { get; init; }

    public required string Message { get; init; }
}