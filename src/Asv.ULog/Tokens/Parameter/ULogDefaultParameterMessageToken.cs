using Asv.IO;

namespace Asv.ULog;

/// <summary>
/// 'Q': Default Parameter Message
///
/// The default parameter message defines the default value of a parameter for a given vehicle and setup.
/// </summary>
public class ULogDefaultParameterMessageToken : ULogParameterMessageToken, IEquatable<ULogDefaultParameterMessageToken>, IULogDefinitionToken, IULogDataToken
{
    public static ULogToken Token => ULogToken.DefaultParameter;
    public new const string Name = "Default Parameter";
    public new const byte TokenId = (byte)'Q';

    public override string TokenName => Name;
    public override ULogToken TokenType => Token;
    public override UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Definition | UTokenPlaceFlags.Data;

    private ULogParameterDefaultTypes _defaultTypes;

    /// <summary>
    /// default_types is a bitfield and defines to which group(s) the value belongs to.
    /// 
    /// At least one bit must be set:
    ///     1&lt;&lt;0: system-wide default
    ///     1&lt;&lt;1: default for the current configuration (e.g. an airframe)
    /// 
    /// A log may not contain default values for all parameters.
    /// In those cases the default is equal to the parameter value, and different default types are treated independently.
    /// </summary>
    public ULogParameterDefaultTypes DefaultType
    {
        get => _defaultTypes;
        set
        {
            CheckDefaultType(value);
            _defaultTypes = value;
        }
    }

    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        DefaultType = (ULogParameterDefaultTypes)BinSerialize.ReadByte(ref buffer);
        base.Deserialize(ref buffer);
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteByte(ref buffer, (byte)DefaultType);
        base.Serialize(ref buffer);
    }

    public override int GetByteSize()
    {
        return sizeof(byte) + base.GetByteSize();
    }

    private void CheckDefaultType(ULogParameterDefaultTypes defaultType)
    {
        if (defaultType != ULogParameterDefaultTypes.None) return;

        throw new ULogException("Default parameter type is None");
    }

    public bool Equals(ULogDefaultParameterMessageToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && DefaultType == other.DefaultType;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogDefaultParameterMessageToken)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), (int)DefaultType);
    }
}

[Flags]
public enum ULogParameterDefaultTypes : byte
{
    None = 0,
    SystemWide = 1 << 0,
    ForCurrentConfiguration = 1 << 1
}