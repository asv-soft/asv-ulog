using System.Buffers;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using Asv.ULog.Information;
using Microsoft.Extensions.Logging;

namespace Asv.ULog;

public static class ULogManager
{
    public static readonly Encoding Encoding = Encoding.UTF8;
    /// <summary>
    /// ushort (size) + byte (type)
    /// </summary>
    public const int TokenHeaderSize = 4;
    public static ulong FromDateTimeToUnixMicroseconds(DateTime dateTime)
    {
        var dateTimeOffset = new DateTimeOffset(dateTime.ToUniversalTime());
        return (ulong)(dateTimeOffset.ToUnixTimeMilliseconds() * 1000 + (dateTimeOffset.Ticks % TimeSpan.TicksPerMillisecond) / 10);
    }
    public static ImmutableDictionary<byte, Func<IULogToken>> TokenFactory { get; }

    static ULogManager()
    {
        var builder = ImmutableDictionary.CreateBuilder<byte, Func<IULogToken>>();
        builder.Add(ULogFlagBitsMessageToken.TokenId, () => new ULogFlagBitsMessageToken());
        builder.Add(ULogFormatMessageToken.TokenId, () => new ULogFormatMessageToken());
        builder.Add(ULogParameterMessageToken.TokenId, () => new ULogParameterMessageToken());
        builder.Add(ULogDefaultParameterMessageToken.TokenId, () => new ULogDefaultParameterMessageToken());
        builder.Add(ULogInformationMessageToken.TokenId, () => new ULogInformationMessageToken());
        builder.Add(ULogMultiInformationMessageToken.TokenId, () => new ULogMultiInformationMessageToken());
        builder.Add(ULogUnsubscriptionMessageToken.TokenId, () => new ULogUnsubscriptionMessageToken());
        builder.Add(ULogLoggedStringMessageToken.TokenId, () => new ULogLoggedStringMessageToken());
        builder.Add(ULogSynchronizationMessageToken.TokenId, () => new ULogSynchronizationMessageToken());
        builder.Add(ULogTaggedLoggedStringMessageToken.TokenId, () => new ULogTaggedLoggedStringMessageToken());
        builder.Add(ULogSubscriptionMessageToken.TokenId, () => new ULogSubscriptionMessageToken());
        builder.Add(ULogDropoutMessageToken.TokenId, () => new ULogDropoutMessageToken());
        builder.Add(ULogLoggedDataMessageToken.TokenId, () => new ULogLoggedDataMessageToken());
        TokenFactory = builder.ToImmutable();
    }
    
    public static IULogReader OpenGzip(string filePath, CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
    {
        var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
        var zip = new GZipStream(fileStream, compressionLevel, false);
        var tokenReader = new StreamULogTokenReader(zip);
        return new ULogReader(tokenReader);
    }
    
    public static IULogWriter CreateGzip(string filePath, DateTime timestamp, CompressionLevel compressionLevel = CompressionLevel.SmallestSize,
        int? writeSyncTokenEveryXToken = null)
    {
        var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        var zip = new GZipStream(fileStream, compressionLevel, false);
        var tokenWriter = new ULogStreamTokenWriter(zip , filePath, writeSyncTokenEveryXToken);
        return new ULogWriter(tokenWriter, timestamp, false);
    }
}