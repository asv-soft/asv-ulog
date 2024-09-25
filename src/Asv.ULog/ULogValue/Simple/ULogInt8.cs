using Asv.IO;

namespace Asv.ULog;

public class ULogInt8 : ULogSimple
{
    private sbyte _value;

    public ULogInt8()
    {
        
    }
    public ULogInt8(sbyte value)
    {
        _value = value;
    }

    public override ULogValue Clone()
    {
        return new ULogInt8(_value);
    }

    public override UValueType Type => UValueType.Int8;
    
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        _value = BinSerialize.ReadSByte(ref buffer);
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteSByte(ref buffer, _value);
    }

    public override int GetByteSize() => sizeof(sbyte);
    internal override ValueType GetValue() => _value;
}