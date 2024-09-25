using Asv.IO;

namespace Asv.ULog;

public class ULogDouble : ULogSimple
{
    private double _value;

    public ULogDouble(double value)
    {
        _value = value;
    }

    public ULogDouble()
    {
        
    }

    public override ULogValue Clone()
    {
        return new ULogDouble(_value);
    }

    public override UValueType Type => UValueType.Double;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadDouble(ref buffer, ref _value);
    }
    
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteDouble(ref buffer, _value);
    }
    
    public override int GetByteSize() => sizeof(double);
    internal override ValueType GetValue() => _value;
}