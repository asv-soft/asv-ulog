namespace Asv.ULog;

/// <summary>
/// 'P': Parameter Message
///
/// Parameter message in the Definitions section defines the parameter values of the vehicle when logging is started.
/// It uses the same format as the Information Message.
///
/// If a parameter dynamically changes during runtime, this message can also be used in the Data section as well.
/// </summary>
public class ULogParameterMessageToken : ULogKeyAndValueTokenBase, IEquatable<ULogParameterMessageToken>, IULogDefinitionToken, IULogDataToken
{
    #region Static

    public static ULogToken Type => ULogToken.Parameter;
    public const string Name = "Parameter";
    public const byte TokenId = (byte)'P';

    #endregion

    public override string TokenName => Name;
    public override ULogToken TokenType => Type;
    public override UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Definition | UTokenPlaceFlags.Data;

    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        base.Deserialize(ref buffer);
        if (Key.Type.BaseType != ULogType.Float && Key.Type.BaseType != ULogType.Int32)
            throw new ULogException($"Parameter message value type must be {ULogType.Float} or {ULogType.Int32}");
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(Key);
        if (Key.Type.BaseType != ULogType.Float && Key.Type.BaseType != ULogType.Int32)
            throw new ULogException($"Parameter message value type must be {ULogType.Float} or {ULogType.Int32}");
        base.Serialize(ref buffer);
    }

    public override int GetByteSize()
    {
        ArgumentNullException.ThrowIfNull(Key);
        if (Key.Type.BaseType != ULogType.Float && Key.Type.BaseType != ULogType.Int32)
            throw new ULogException($"Parameter message value type must be {ULogType.Float} or {ULogType.Int32}");
        return base.GetByteSize();
    }

    public bool Equals(ULogParameterMessageToken? other) => base.Equals(other);

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogParameterMessageToken)obj);
    }

    public override int GetHashCode() => base.GetHashCode();
}