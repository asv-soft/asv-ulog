using Asv.IO;

namespace Asv.ULog;

public class ULogInt64 : ULogSimple
{
    private long _value;

    public ULogInt64(long value)
    {
        _value = value;
    }

    public ULogInt64()
    {
        
    }

    public override ULogValue CloneToken()
    {
        return new ULogInt64(_value);
    }

    public override UValueType Type => UValueType.Int64;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadLong(ref buffer, ref _value);
    }
    
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteLong(ref buffer, _value);
    }
    
    public override int GetByteSize() => sizeof(long);
    internal override ValueType GetValue() => _value;
}