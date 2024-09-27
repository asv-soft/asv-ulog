using Asv.IO;

namespace Asv.ULog;

public class ULogUInt64 : ULogSimple
{
    private ulong _value;

    public ULogUInt64(ulong value)
    {
        _value = value;
    }

    public ULogUInt64()
    {
        
    }

    public override ULogValue CloneToken()
    {
        return new ULogUInt64(_value);
    }

    public override UValueType Type => UValueType.UInt64;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadULong(ref buffer, ref _value);
    }
    
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteULong(ref buffer, _value);
    }
    
    public override int GetByteSize() => sizeof(ulong);
    internal override ValueType GetValue() => _value;
}