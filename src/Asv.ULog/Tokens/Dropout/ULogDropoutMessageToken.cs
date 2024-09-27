using Asv.IO;

namespace Asv.ULog;

/// <summary>
/// 'O': Dropout Message
///
/// Mark a dropout (lost logging messages) of a given duration in ms.
/// Dropouts can occur e.g. if the device is not fast enough.
/// </summary>
public class ULogDropoutMessageToken : IULogToken, IEquatable<ULogDropoutMessageToken>
{
    public static ULogToken Token => ULogToken.Dropout;
    public const string Name = "Dropout message";
    public const byte TokenId = (byte)'O';

    public string TokenName => Name;
    public ULogToken TokenType => Token;
    public TokenPlaceFlags TokenSection => TokenPlaceFlags.Data;

    /// <summary>
    /// Duration of the lost logging messages in ms.
    /// </summary>
    public ushort Duration { get; set; }

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        Duration = BinSerialize.ReadUShort(ref buffer);
    }

    public void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteUShort(ref buffer, Duration);
    }

    public int GetByteSize()
    {
        return sizeof(ushort);
    }

    public bool Equals(ULogDropoutMessageToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Duration == other.Duration;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogDropoutMessageToken)obj);
    }

    public override int GetHashCode()
    {
        return Duration.GetHashCode();
    }
}