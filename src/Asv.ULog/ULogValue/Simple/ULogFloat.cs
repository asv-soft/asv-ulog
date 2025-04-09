using System.Runtime.CompilerServices;
using Asv.IO;

namespace Asv.ULog;

public class ULogFloat : ULogSimple
{
    private float _value;

    public ULogFloat(float value)
    {
        _value = value;
    }

    public ULogFloat()
    {
        
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ULogValue CloneToken()
    {
        return new ULogFloat(_value);
    }

    public override UValueType Type => UValueType.Float;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadFloat(ref buffer, ref _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteFloat(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(float);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
}