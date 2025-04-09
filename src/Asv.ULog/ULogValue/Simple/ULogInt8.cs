using System.Runtime.CompilerServices;
using Asv.IO;

namespace Asv.ULog;

public class ULogInt8 : ULogSimple
{
    private sbyte _value;

    public ULogInt8()
    {
        
    }
    public ULogInt8(sbyte value)
    {
        _value = value;
    }

    public override ULogValue CloneToken()
    {
        return new ULogInt8(_value);
    }

    public override UValueType Type => UValueType.Int8;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        _value = BinSerialize.ReadSByte(ref buffer);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteSByte(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(sbyte);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string? ToString() => _value.ToString();
}