using System.Runtime.CompilerServices;
using Asv.IO;

namespace Asv.ULog;

public class ULogBool : ULogSimple
{
    private bool _value;

    public ULogBool(bool value)
    {
        _value = value;
    }

    public ULogBool()
    {
    }

    public override ULogValue CloneToken()
    {
        return new ULogBool(_value);
    }

    
    public override UValueType Type => UValueType.Bool;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        BinSerialize.ReadBool(ref buffer, ref _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteBool(ref buffer, _value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetByteSize() => sizeof(byte);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override ValueType GetValue() => _value;
    
}