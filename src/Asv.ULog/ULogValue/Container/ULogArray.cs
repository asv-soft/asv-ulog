using System.Collections.Immutable;

namespace Asv.ULog;

public class ULogArray : ULogContainer
{
    private readonly ImmutableArray<ULogValue> _values;
    private readonly int _size;

    public ULogArray(ImmutableArray<ULogValue> values)
    {
        _values = values;
        _size = 0;
        foreach (var value in _values)
        {
            value.Parent = this;
            _size += value.GetByteSize();
        }
    }
    public override ULogValue CloneToken()
    {
        var newValues = ImmutableArray.CreateBuilder<ULogValue>(_values.Length);
        foreach (var value in _values)
        {
            newValues.Add(value.CloneToken());
        }
        return new ULogArray(newValues.ToImmutable());
    }

    public override UValueType Type => UValueType.Array;
    public IList<ULogValue> Items => _values;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        foreach (var v in _values)
        {
            v.Deserialize(ref buffer);
        }
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        foreach (var v in _values)
        {
            v.Serialize(ref buffer);
        }
    }

    public override int GetByteSize() => _size;
    
    public override string ToString()
    {
        return $"[{string.Join(", ", _values)}]";
    }
}