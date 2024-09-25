using Asv.IO;

namespace Asv.ULog;

/// <summary>
/// 'R': Unsubscription Message
///
/// Unsubscribe a message, to mark that it will not be logged anymore (not used currently).
/// </summary>
public class ULogUnsubscriptionMessageToken : IULogToken, IEquatable<ULogUnsubscriptionMessageToken>
{
    #region Static

    public static ULogToken Type => ULogToken.Unsubscription;
    public const string Name = "Unsubscription";
    public const byte TokenId = (byte)'R';

    #endregion

    public string TokenName => Name;
    public ULogToken TokenType => Type;
    public TokenPlaceFlags TokenSection => TokenPlaceFlags.Data;

    /// <summary>
    /// ID of the message
    /// </summary>
    public ushort MessageId;

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        MessageId = BinSerialize.ReadUShort(ref buffer);
    }

    public void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteUShort(ref buffer, MessageId);
    }

    public int GetByteSize()
    {
        return sizeof(ushort) /*MessageId*/;
    }

    public bool Equals(ULogUnsubscriptionMessageToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return MessageId == other.MessageId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogUnsubscriptionMessageToken)obj);
    }

    public override int GetHashCode()
    {
        return MessageId.GetHashCode();
    }
}