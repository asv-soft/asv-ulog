namespace Asv.ULog;

public abstract class ULogSimple:ULogValue
{
    internal abstract ValueType GetValue();
    public override string? ToString() => GetValue().ToString();
}