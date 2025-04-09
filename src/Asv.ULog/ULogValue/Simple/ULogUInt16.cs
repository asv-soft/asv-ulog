using System.Runtime.CompilerServices;
using Asv.IO;

namespace Asv.ULog;

public class ULogUInt16 : ULogSimple
{
    private ushort _value;

    public ULogUInt16()
    {
        
    }

    public ULogUInt16(ushort value)
    {
        _value = value;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ULogValue CloneToken()
    {
        return new ULogUInt16(_value);
    }

    public override UValueType Type => UValueType.UInt16;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadUShort(ref buffer, ref _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteUShort(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(ushort);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
}