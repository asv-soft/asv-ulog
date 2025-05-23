using System.Runtime.CompilerServices;
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

    public override ULogValue CloneToken()
    {
        return new ULogUInt32(_value);
    }

    public override UValueType Type => UValueType.UInt32;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadUInt(ref buffer, ref _value);
    }[MethodImpl(MethodImplOptions.AggressiveInlining)]
    
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteUInt(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(uint);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
}