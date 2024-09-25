namespace Asv.ULog;

public class ULogArray : ULogContainer
{
    public override ULogValue Clone()
    {
        var array = new ULogArray();
        foreach (var v in _values)
        {
            array.Items.Add(v.Clone());
        }

        return array;
    }

    public override UValueType Type => UValueType.Array;

    private readonly List<ULogValue> _values = new();
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

    public override int GetByteSize() => _values.Sum(x => x.GetByteSize());
}