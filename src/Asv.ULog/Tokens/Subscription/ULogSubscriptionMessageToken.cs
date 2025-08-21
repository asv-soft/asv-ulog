using System.Buffers;
using Asv.IO;

namespace Asv.ULog;

/// <summary>
/// 'A': Subscription Message
///
/// Subscribe a message by name and give it an id that is used in Logged data Message.
/// This must come before the first corresponding Logged data Message.
/// </summary>
public class ULogSubscriptionMessageToken : IULogToken, IEquatable<ULogSubscriptionMessageToken>, IULogDefinitionToken, IULogDataToken
{
    public static ULogToken Type = ULogToken.Subscription;
    public const string Name = "Subscription";
    public const byte TokenId = (byte)'A';

    private string _messageName = null!;
    public string TokenName => Name;
    public ULogToken TokenType => Type;
    public UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Definition | UTokenPlaceFlags.Data;

    /// <summary>
    /// The same message format can have multiple instances, for example if the system has two sensors of the same type.
    /// 
    /// The default and first instance must be 0.
    /// </summary>
    public byte MultiId { get; set; }

    /// <summary>
    /// Unique id to match Logged data Message data. The first use must set this to 0, then increase it.
    ///
    /// The same msg_id must not be used twice for different subscriptions.
    /// </summary>
    public ushort MessageId { get; set; }

    /// <summary>
    /// Message name to subscribe to. Must match one of the Format Message definitions.
    /// </summary>
    public string MessageName
    {
        get => _messageName;
        set
        {
            ULogFormatMessageToken.CheckMessageName(value);
            _messageName = value;
        }
    }

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        MultiId = BinSerialize.ReadByte(ref buffer);
        MessageId = BinSerialize.ReadUShort(ref buffer);
        var charSize = ULogManager.Encoding.GetCharCount(buffer);
        var charBuffer = ArrayPool<char>.Shared.Rent(charSize);
        try
        {
            ULogManager.Encoding.GetChars(buffer, charBuffer);
            MessageName = new ReadOnlySpan<char>(charBuffer, 0, charSize).Trim().ToString();
            buffer = buffer[MessageName.Length..];
        }
        finally
        {
            ArrayPool<char>.Shared.Return(charBuffer);
        }
    }

    public void Serialize(ref Span<byte> buffer)
    {
        ULogFormatMessageToken.CheckMessageName(MessageName);
        BinSerialize.WriteByte(ref buffer, MultiId);
        BinSerialize.WriteUShort(ref buffer, MessageId);
        BinSerialize.WriteBlock(ref buffer, ULogManager.Encoding.GetBytes(MessageName));
    }

    public int GetByteSize()
    {
        return sizeof(byte) + sizeof(ushort) + ULogManager.Encoding.GetByteCount(MessageName);
    }

    public bool Equals(ULogSubscriptionMessageToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return MessageName == other.MessageName && MultiId == other.MultiId && MessageId == other.MessageId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogSubscriptionMessageToken)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MessageName, MultiId, MessageId);
    }
}