using Asv.IO;

namespace Asv.ULog;

public class ULogInt16 : ULogSimple
{
    private short _value;

    public ULogInt16()
    {
        
    }

    public ULogInt16(short value)
    {
        _value = value;
    }

    public override ULogValue CloneToken()
    {
        return new ULogInt16(_value);
    }

    public override UValueType Type => UValueType.Int16;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadShort(ref buffer,ref _value);
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteShort(ref buffer, _value);
    }

    public override int GetByteSize() => sizeof(short);
    internal override ValueType GetValue() => _value;
}