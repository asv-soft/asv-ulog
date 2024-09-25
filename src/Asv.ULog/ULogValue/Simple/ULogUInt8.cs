using Asv.IO;

namespace Asv.ULog;

public class ULogUInt8 : ULogSimple
{
    private byte _value;

    public ULogUInt8()
    {
        
    }

    public ULogUInt8(byte value)
    {
        _value = value;
    }

    public override ULogValue Clone()
    {
        return new ULogUInt8(_value);
    }

    public override UValueType Type => UValueType.UInt8;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadByte(ref buffer, ref _value);
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteByte(ref buffer, _value);
    }

    public override int GetByteSize() => sizeof(byte);
    internal override ValueType GetValue() => _value;
}