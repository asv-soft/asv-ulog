namespace Asv.ULog;

public interface IULogWriter : IDisposable
{
    string SourceName { get; }
    IULogWriter AppendHeader(ULogFileHeaderToken header, ULogFlagBitsMessageToken flags);
    IULogWriter AppendDefinition(IULogDefinitionToken definitionToken);
    IULogWriter AppendData(IULogDataToken dataToken);
}

public static class ULogWriterMixin
{
    public static IULogWriter AppendHeader(this IULogWriter writer, DateTime timestamp)
    {
        var header = new ULogFileHeaderToken
        {
            Version = 1,
            Timestamp = ULog.FromDateTimeToUnixMicroseconds(timestamp),
        };
        var flags = new ULogFlagBitsMessageToken
        {
            AppendedOffsets = [],
            CompatFlags = [],
            IncompatFlags = []
        };
        return writer.AppendHeader(header, flags);

    }
}