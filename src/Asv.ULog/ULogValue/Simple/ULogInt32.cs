using System.Runtime.CompilerServices;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ULogValue CloneToken()
    {
        return new ULogInt32(_value);
    }

    public override UValueType Type => UValueType.Int32;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadInt(ref buffer, ref _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteInt(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(int);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
}