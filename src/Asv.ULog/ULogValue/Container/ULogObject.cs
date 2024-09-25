namespace Asv.ULog;

public class ULogObject : ULogContainer
{
    private readonly List<ULogProperty> _properties = new();
    public override ULogValue Clone()
    {
        var obj = new ULogObject();
        foreach (var p in _properties)
        {
            obj.Properties.Add((ULogProperty)p.Clone());
        }

        return obj;
    }

    public override UValueType Type => UValueType.Object;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        foreach(var p in _properties)
        {
            p.Deserialize(ref buffer);
        }
    }
    
    public IList<ULogProperty> Properties => _properties;

    public override void Serialize(ref Span<byte> buffer)
    {
        foreach (var p in _properties)
        {
            p.Serialize(ref buffer);
        }
    }

    public override int GetByteSize() => _properties.Sum(x => x.GetByteSize());

    
}