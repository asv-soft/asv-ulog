namespace Asv.ULog;

public class ULogUnknownToken : IULogToken, IEquatable<ULogUnknownToken>
{
    #region Static

    public const ULogToken Type = ULogToken.Unknown;
    public const string Name = "Unknown";

    #endregion

    private readonly ushort _byteSize;
    private byte _unknownType;

    public ULogUnknownToken(byte type, ushort byteSize)
    {
        _byteSize = byteSize;
        UnknownType = type;
    }

    public string TokenName => Name;
    public ULogToken TokenType => Type;
    public TokenPlaceFlags TokenSection => TokenPlaceFlags.DefinitionAndData;

    public char UnknownTypeChar { get; private set; }

    public byte UnknownType
    {
        get => _unknownType;
        set
        {
            _unknownType = value;
            var buff = new char[1];
            ULog.Encoding.GetChars(new ReadOnlySpan<byte>([_unknownType]), new Span<char>(buff));
            UnknownTypeChar = buff[0];
        }
    }

    public byte[] Data { get; set; }

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        Data = buffer.ToArray();
    }

    public void Serialize(ref Span<byte> buffer)
    {
        Data.CopyTo(buffer);
        buffer = buffer[Data.Length..];
    }

    public int GetByteSize()
    {
        return _byteSize;
    }

    public bool Equals(ULogUnknownToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _byteSize == other._byteSize && UnknownType == other.UnknownType && 
               UnknownTypeChar == other.UnknownTypeChar && Data.Equals(other.Data);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogUnknownToken)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_byteSize, UnknownType, UnknownTypeChar, Data);
    }
}