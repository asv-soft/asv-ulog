using System.Collections.Immutable;

namespace Asv.ULog;

public static class ULogValueMixin
{
    public static byte[] ToByteArray(this ULogValue src)
    {
        var result = new byte[src.GetByteSize()];
        var span = new Span<byte>(result);
        src.Serialize(ref span);
        return result;
    }
    
    public static ULogValue CreateNonReferenceType(ULogTypeDefinition fieldType)
    {
        if (fieldType.BaseType == ULogType.ReferenceType)
        {
            throw new ArgumentException("Field type is reference type", nameof(fieldType));
        }
        var value = CreateSimple(fieldType);
        if (!fieldType.IsArray) return value;
        var buidler = ImmutableArray.CreateBuilder<ULogValue>(fieldType.ArraySize);
        buidler.Add(value);
        for (var i = 1; i < fieldType.ArraySize; i++)
        {
            buidler.Add(value.CloneToken());    
        }

        return new ULogArray(buidler.ToImmutable());
    }
    
    public static ULogValue Create(ULogLoggedDataMessageToken data,IReadOnlyDictionary<string, ULogFormatMessageToken> messages,
        IReadOnlyDictionary<ushort, ULogSubscriptionMessageToken> subscriptions, out string messageName)
    {
        var sub = subscriptions[data.MessageId];
        var message = messages[sub.MessageName];
        messageName = sub.MessageName;
        var obj = CreateReference(message.MessageName,messages);
        var span = new ReadOnlySpan<byte>(data.Data.Array, data.Data.Offset, data.Data.Count);
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
        => fieldType.BaseType == ULogType.ReferenceType ? CreateReference(fieldType.TypeName, messages) : CreateNonReferenceType(fieldType);

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