using System.Buffers;
using System.Collections.Immutable;
using System.Text;
using Asv.ULog.Information;
using Microsoft.Extensions.Logging;

namespace Asv.ULog;

public static class ULog
{
    public static readonly Encoding Encoding = Encoding.UTF8;
    /// <summary>
    /// ushort (size) + byte (type)
    /// </summary>
    public const int TokenHeaderSize = 4;
    public static IULogReader CreateReader(ILogger? logger = null)
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
        return new ULogReader(builder.ToImmutable(), logger);
    }

    public static IULogWriter CreateWriter(IBufferWriter<byte> buffer, string sourceName, int? writeSyncTokenEveryXTokens = null, bool disposeStream = true)
    {
        return new ULogBufferWriter(buffer, sourceName, writeSyncTokenEveryXTokens,disposeStream);
    }
    
    public static IULogWriter CreateWriter(Stream stream, string sourceName, int? writeSyncTokenEveryXTokens = null, bool disposeStream = true)
    {
        return new ULogStreamWriter(stream, sourceName, writeSyncTokenEveryXTokens,disposeStream);
    }
    
    public static ulong FromDateTimeToUnixMicroseconds(DateTime dateTime)
    {
        var dateTimeOffset = new DateTimeOffset(dateTime.ToUniversalTime());
        return (ulong)(dateTimeOffset.ToUnixTimeMilliseconds() * 1000 + (dateTimeOffset.Ticks % TimeSpan.TicksPerMillisecond) / 10);
    }
    
    // TODO: remove this by Create ULogValue
    public static ValueType GetSimpleValue(ULogType type, byte[] value)
    {
        switch (type)
        {
            case ULogType.Float:
                return BitConverter.ToSingle(value);
            case ULogType.Int32:
                return BitConverter.ToInt32(value);
            case ULogType.UInt32:
                return BitConverter.ToUInt32(value);
            case ULogType.Char:
                return BitConverter.ToChar(value);
            case ULogType.Int8:
                return (sbyte)value[0];
            case ULogType.UInt8:
                return value[0];
            case ULogType.Int16:
                return BitConverter.ToInt16(value);
            case ULogType.UInt16:
                return BitConverter.ToUInt16(value);
            case ULogType.Int64:
                return BitConverter.ToInt64(value);
            case ULogType.UInt64:
                return BitConverter.ToUInt64(value);
            case ULogType.Double:
                return BitConverter.ToDouble(value);
            case ULogType.Bool:
                return value[0] != 0;
            case ULogType.ReferenceType:
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
        
    
}