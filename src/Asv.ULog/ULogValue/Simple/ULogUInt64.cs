using System.Runtime.CompilerServices;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ULogValue CloneToken()
    {
        return new ULogUInt64(_value);
    }

    public override UValueType Type => UValueType.UInt64;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadULong(ref buffer, ref _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteULong(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(ulong);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
}