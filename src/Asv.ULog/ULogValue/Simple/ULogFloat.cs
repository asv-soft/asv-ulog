using Asv.IO;

namespace Asv.ULog;

public class ULogFloat : ULogSimple
{
    private float _value;

    public ULogFloat(float value)
    {
        _value = value;
    }

    public ULogFloat()
    {
        
    }

    public override ULogValue Clone()
    {
        return new ULogFloat(_value);
    }

    public override UValueType Type => UValueType.Float;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadFloat(ref buffer, ref _value);
    }
    
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteFloat(ref buffer, _value);
    }
    
    public override int GetByteSize() => sizeof(float);
    internal override ValueType GetValue() => _value;
}