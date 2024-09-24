namespace Asv.ULog.ULogFile.Processors;

public sealed class CompositeKey
{
    public required ULogTypeDefinition Type { get; init; }
    public required string Name { get; init; }
}