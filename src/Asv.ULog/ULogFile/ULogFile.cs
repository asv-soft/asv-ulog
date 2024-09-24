using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using Asv.ULog.ULogFile.Processors;

namespace Asv.ULog.ULogFile;

public class Data
{
    public IDictionary<ushort, SubscriptionMessage> SubscriptionMessages { get; } = new Dictionary<ushort, SubscriptionMessage>();
    public IDictionary<ushort, ULogArray> LoggedDataMessages { get; } = new Dictionary<ushort, ULogArray>();
    public ICollection<LoggedStringMessage> LoggedStringMessages { get; } = new List<LoggedStringMessage>();
    public IDictionary<ushort, TaggedLoggedStringMessage> TaggedLoggedStringMessages { get; } = new Dictionary<ushort, TaggedLoggedStringMessage>();
    public ICollection<SynchronizationMessage> SynchronizationMessages { get; } = new List<SynchronizationMessage>();
    public ICollection<ushort> DropoutMessages { get; } = new List<ushort>();
    
    public IDictionary<CompositeKey, string> InformationMessages { get; } = new Dictionary<CompositeKey, string>();
    public IDictionary<CompositeKey, ICollection<string>> MultiInformationMessages { get; } = new Dictionary<CompositeKey, ICollection<string>>();
    public IDictionary<CompositeKey, string> Parameters { get; } = new Dictionary<CompositeKey, string>();
    public IDictionary<CompositeKey, string> DefaultParameters { get; } = new Dictionary<CompositeKey, string>();
}

public class Definition
{
    public ULogFlagBitsMessageToken FlagBits { get; } = new();
    public ICollection<IDictionary<string, ULogValue>> FormatMessages { get; } = new List<IDictionary<string, ULogValue>>();
    public IDictionary<CompositeKey, string> InformationMessages { get; } = new Dictionary<CompositeKey, string>();
    public IDictionary<CompositeKey, ICollection<string>> MultiInformationMessages { get; } = new Dictionary<CompositeKey, ICollection<string>>();
    public IDictionary<CompositeKey, string> Parameters { get; } = new Dictionary<CompositeKey, string>();
    public IDictionary<CompositeKey, string> DefaultParameters { get; } = new Dictionary<CompositeKey, string>();
}

public class ULogFile
{
    private readonly IULogReader _reader;
    private readonly TokenProcessor _processor;

    /*
        Header
    */
    public TimeSpan Time { get; set; }
    public byte Version { get; set; }
    
    
    public readonly Definition Definition = new();
    public readonly Data Data = new();
    
    public TokenPlaceFlags ReadState { get; private set; } = TokenPlaceFlags.None;

    public ULogFile(IULogReader reader)
    {
        _reader = reader;
        _processor = new TokenProcessor(new Dictionary<ULogToken, ITokenHandler>
        {
            { ULogToken.FileHeader, new FileHeaderTokenHandler() },
            { ULogToken.FlagBits, new FlagBitsTokenHandler() },
            { ULogToken.Unsubscription, new EmptyTokenHandler() } // cause unsub is not used currently
        }, new ProcessorContext{ File = this });
    }
    
    public void Deserialize(ref ReadOnlySequence<byte> data)
    {
        ReadState = TokenPlaceFlags.Header;
        var rdr = new SequenceReader<byte>(data);
        while (_reader.TryRead(ref rdr, out var token))
        {
            Debug.Assert(token is not null);
            ReadState = _processor.Process(token);
        }
    }

    public void Serialize(ref Span<byte> buffer)
    {
        throw new NotImplementedException();
    }
        
    public static ULogValue Create(ULogLoggedDataMessageToken data,IReadOnlyDictionary<string, ULogFormatMessageToken> messages,
        IReadOnlyDictionary<ushort, ULogSubscriptionMessageToken> subscriptions, out string messageName)
    {
        var sub = subscriptions[data.MessageId];
        var message = messages[sub.MessageName];
        messageName = sub.MessageName;
        var obj = CreateReference(message.MessageName,messages);
        var span = new ReadOnlySpan<byte>(data.Data);
        obj.Deserialize(ref span);
        return obj;
    }

    private static ULogObject CreateReference(string name, IReadOnlyDictionary<string, ULogFormatMessageToken> messages)
    {
        var message = messages[name];
        var builder = ImmutableArray.CreateBuilder<ULogProperty>(message.Fields.Count);
        foreach (var field in message.Fields)
        {
            var prop = Create(field.Type, messages);
            builder.Add(new ULogProperty(field.Name,prop));
        }
        if (message.Fields[^1].Name.StartsWith("_padding"))
        {
            builder.RemoveAt(builder.Count - 1);
        }
        return new ULogObject(builder.ToImmutable());
    }

    private static ULogValue Create(ULogTypeDefinition fieldType, IReadOnlyDictionary<string, ULogFormatMessageToken> messages)
    {
        ULogValue value;
        if (fieldType.BaseType == ULogType.ReferenceType)
        {
            value = CreateReference(fieldType.TypeName, messages);
        }
        else
        {
            value = CreateSimple(fieldType);
        }

        if (!fieldType.IsArray) return value;

        var buidler = ImmutableArray.CreateBuilder<ULogValue>(fieldType.ArraySize);
        buidler.Add(value);
        for (var i = 1; i < fieldType.ArraySize; i++)
        {
            buidler.Add(value.CloneToken());    
        }

        return new ULogArray(buidler.ToImmutable());

    }

    private static ULogSimple CreateSimple(ULogTypeDefinition type)
    {
        switch (type.BaseType)
        {
            case ULogType.Int8:
                return new ULogInt8();
            case ULogType.UInt8:
                return new ULogUInt8();
            case ULogType.Int16:
                return new ULogInt16();
            case ULogType.UInt16:
                return new ULogUInt16();
            case ULogType.Int32:
                return new ULogInt32();
            case ULogType.UInt32:
                return new ULogUInt32();
            case ULogType.Int64:
                return new ULogInt64();
            case ULogType.UInt64:
                return new ULogUInt64();
            case ULogType.Float:
                return new ULogFloat();
            case ULogType.Double:
                return new ULogDouble();
            case ULogType.Bool:
                return new ULogBool();
            case ULogType.Char:
                return new ULogChar();
            case ULogType.ReferenceType:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
