using System.Runtime.CompilerServices;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ULogValue CloneToken()
    {
        return new ULogInt64(_value);
    }

    public override UValueType Type => UValueType.Int64;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadLong(ref buffer, ref _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteLong(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(long);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
}