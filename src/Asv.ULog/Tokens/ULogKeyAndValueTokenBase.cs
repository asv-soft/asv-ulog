using Asv.IO;

namespace Asv.ULog;

/// <summary>
/// Base class for ULog key-value tokens.
///
/// e.g. Information, Parameter, etc.
/// 
/// uint8_t key_len;
/// char key[key_len];
/// char value[header.msg_size-2-key_len]
/// </summary>
public abstract class ULogKeyAndValueTokenBase : IULogToken, IEquatable<ULogKeyAndValueTokenBase>
{
    #region Static

    private const byte ArrayStart = (byte)'[';
    private const byte ArrayEnd = (byte)']';

    #endregion
    
    public virtual void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        var keyLen = BinSerialize.ReadByte(ref buffer);
        var key = buffer[..keyLen];
        Key = new ULogTypeAndNameDefinition();
        Key.Deserialize(ref key);
        buffer = buffer[keyLen..];
        Value = ULogValueMixin.CreateNonReferenceType(Key.Type);
    }

    public virtual void Serialize(ref Span<byte> buffer)
    {
        var arraySizeStr = Key.Type.IsArray ? $"[{Key.Type.ArraySize}] " : " ";
        var additionalSymbolsSize = arraySizeStr.Length;
        BinSerialize.WriteByte(ref buffer, (byte)(Key.Type.TypeName.Length + additionalSymbolsSize + Key.Name.Length));
        Key.Serialize(ref buffer);
        Value.Serialize(ref buffer);
    }

    public virtual int GetByteSize()
    {
        return sizeof(byte) + Key.GetByteSize() + Value.GetByteSize();
    }

    public abstract string TokenName { get; }
    public abstract ULogToken TokenType { get; }
    public abstract UTokenPlaceFlags TokenSection { get; }

    public ULogTypeAndNameDefinition Key { get; set; } = null!;
    public ULogValue Value { get; set; }

    public bool Equals(ULogKeyAndValueTokenBase? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return TokenName == other.TokenName && TokenType == other.TokenType && TokenSection == other.TokenSection && 
               Key.Equals(other.Key) && Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogKeyAndValueTokenBase)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TokenName, (int)TokenType, (int)TokenSection, Key, Value);
    }
}