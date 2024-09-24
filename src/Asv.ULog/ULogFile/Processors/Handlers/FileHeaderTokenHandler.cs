using System.Diagnostics;

namespace Asv.ULog.ULogFile.Processors;

public class FileHeaderTokenHandler : ITokenHandler
{
    public void Handle(IULogToken token, ProcessorContext context)
    {
        Debug.Assert(context.File.ReadState == TokenPlaceFlags.Header);
        var header = token as ULogFileHeaderToken;

        context.File.Time = FromTimeStampToDateTime(header!.Timestamp).TimeOfDay;
        context.File.Version = header.Version;
    }
    
    private static DateTimeOffset FromTimeStampToDateTime(ulong timestamp)
    {
        var dateTimeOffset = new DateTimeOffset();
        dateTimeOffset = dateTimeOffset.AddSeconds(timestamp);
        return dateTimeOffset;
    }
}