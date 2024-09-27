using Asv.IO;

namespace Asv.ULog.Information;

/// <summary>
/// 'M': Multi Information Message
/// Multi information message serves the same purpose as the information message, but for long messages or multiple messages with the same key.
/// </summary>
public class ULogMultiInformationMessageToken : ULogKeyAndValueTokenBase, IEquatable<ULogMultiInformationMessageToken>
{
    #region Static

    public static ULogToken Type => ULogToken.MultiInformation;
    public const string Name = "MultiInformation";
    public const byte TokenId = (byte)'M';

    #endregion

    private byte _isContinued;
    public override string TokenName => Name;
    public override ULogToken TokenType => Type;
    public override TokenPlaceFlags TokenSection => TokenPlaceFlags.Definition | TokenPlaceFlags.Data;

    /// <summary>
    /// IsContinued can be used for split-up messages: if set to 1, it is part of the previous message with the same key.
    /// </summary>
    public byte IsContinued
    {
        get => _isContinued;
        set => _isContinued = value;
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteByte(ref buffer, _isContinued);
        base.Serialize(ref buffer);
    }

    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadByte(ref buffer, ref _isContinued);
        base.Deserialize(ref buffer);
    }

    public override int GetByteSize()
    {
        return base.GetByteSize() + sizeof(byte) /* IsContinued */;
    }

    public bool Equals(ULogMultiInformationMessageToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && IsContinued == other.IsContinued;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogMultiInformationMessageToken)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), IsContinued);
    }
}