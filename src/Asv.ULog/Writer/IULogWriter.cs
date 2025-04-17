using System.Buffers;
using System.Runtime.CompilerServices;

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

   
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IULogWriter AppendDefaultParameter(
        this IULogWriter writer, 
        string name, 
        int value, 
        ULogParameterDefaultTypes type = ULogParameterDefaultTypes.None)
    {
        return writer.AppendDefinition(new ULogDefaultParameterMessageToken
        {
            DefaultType = type,
            Key = new ULogTypeAndNameDefinition
            {
                Type = new ULogTypeDefinition
                {
                    BaseType = ULogType.Int32,
                    TypeName = ULogTypeDefinition.Int32TypeName,
                    ArraySize = 0,
                },
                Name = name,
            },
            Value = new ULogInt32(value).ToByteArray()
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IULogWriter AppendDefaultParameter(
        this IULogWriter writer, 
        string name, 
        float value, 
        ULogParameterDefaultTypes type = ULogParameterDefaultTypes.None)
    {
        return writer.AppendDefinition(new ULogDefaultParameterMessageToken
        {
            DefaultType = type,
            Key = new ULogTypeAndNameDefinition
            {
                Type = new ULogTypeDefinition
                {
                    BaseType = ULogType.Int32,
                    TypeName = ULogTypeDefinition.Int32TypeName,
                    ArraySize = 0,
                },
                Name = name,
            },
            Value = new ULogFloat(value).ToByteArray()
        });
    }
}