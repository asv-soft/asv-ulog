namespace Asv.ULog;

public class ULogProperty :ULogContainer
{
    public ULogProperty(string name, ULogValue value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; set; }
    public override ULogValue Clone()
    {
        return new ULogProperty(Name, Value.Clone());
    }

    public override UValueType Type => UValueType.Property;
    public ULogValue Value { get; set; }
    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        Value.Deserialize(ref buffer);
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        Value.Serialize(ref buffer);
    }

    public override int GetByteSize()
    {
        return Value.GetByteSize();
    }
}