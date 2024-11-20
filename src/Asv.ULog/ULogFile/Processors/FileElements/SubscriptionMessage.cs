namespace Asv.ULog.ULogFile.Processors;

public sealed class SubscriptionMessage
{
    public required byte MultiId { get; init; }
    public required string MessageName { get; init; }
}