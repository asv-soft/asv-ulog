using Asv.IO;

namespace Asv.ULog;

public class ULogInt32 : ULogSimple
{
    private int _value;

    public ULogInt32()
    {
        
    }

    public ULogInt32(int value)
    {
        _value = value;
    }

    public override ULogValue CloneToken()
    {
        return new ULogInt32(_value);
    }

    public override UValueType Type => UValueType.Int32;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadInt(ref buffer, ref _value);
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteInt(ref buffer, _value);
    }

    public override int GetByteSize() => sizeof(int);
    internal override ValueType GetValue() => _value;
}