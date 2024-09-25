using Asv.IO;

namespace Asv.ULog;

public class ULogUInt32 : ULogSimple
{
    private uint _value;

    public ULogUInt32()
    {
        
    }

    public ULogUInt32(uint value)
    {
        _value = value;
    }

    public override ULogValue Clone()
    {
        return new ULogUInt32(_value);
    }

    public override UValueType Type => UValueType.UInt32;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadUInt(ref buffer, ref _value);
    }
    
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteUInt(ref buffer, _value);
    }
    
    public override int GetByteSize() => sizeof(uint);
    internal override ValueType GetValue() => _value;
}