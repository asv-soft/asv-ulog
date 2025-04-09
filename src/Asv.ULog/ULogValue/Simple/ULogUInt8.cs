using System.Runtime.CompilerServices;
using Asv.IO;

namespace Asv.ULog;

public class ULogUInt8 : ULogSimple
{
    private byte _value;

    public ULogUInt8()
    {
        
    }

    public ULogUInt8(byte value)
    {
        _value = value;
    }

    public override ULogValue CloneToken()
    {
        return new ULogUInt8(_value);
    }

    public override UValueType Type => UValueType.UInt8;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadByte(ref buffer, ref _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteByte(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(byte);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
}