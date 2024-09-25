using Asv.IO;

namespace Asv.ULog;

public class ULogUInt16 : ULogSimple
{
    private ushort _value;

    public ULogUInt16()
    {
        
    }

    public ULogUInt16(ushort value)
    {
        _value = value;
    }

    public override ULogValue Clone()
    {
        return new ULogUInt16(_value);
    }

    public override UValueType Type => UValueType.UInt16;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadUShort(ref buffer, ref _value);
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteUShort(ref buffer, _value);
    }

    public override int GetByteSize() => sizeof(ushort);
    internal override ValueType GetValue() => _value;
}