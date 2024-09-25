namespace Asv.ULog;

public class ULogProperty :ULogContainer
{
    private ULogValue _value;

    public ULogProperty(string name, ULogValue value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Name { get; set; }

    public ULogValue Value
    {
        get => _value;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _value.Parent = this;
            _value = value;
        }
    }

    public override ULogValue CloneToken() => new ULogProperty(Name, Value.CloneToken());
    public override UValueType Type => UValueType.Property;
    public override void Deserialize(ref ReadOnlySpan<byte> buffer) => Value.Deserialize(ref buffer);
    public override void Serialize(ref Span<byte> buffer) => Value.Serialize(ref buffer);
    public override int GetByteSize() => Value.GetByteSize();
}