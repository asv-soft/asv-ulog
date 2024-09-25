namespace Asv.ULog;

public class ULogChar : ULogSimple
{
    private char _value;

    public ULogChar(char value)
    {
        _value = value;
    }

    public ULogChar()
    {
        
    }

    public override ULogValue CloneToken()
    {
        return new ULogChar(_value);
    }

    public override UValueType Type => UValueType.Char;

    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        Span<char> span = stackalloc char[1];
        ULog.Encoding.GetChars(buffer[..1], span);
        _value = span[0];
        buffer = buffer[1..];
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        Span<char> span = stackalloc char[1];
        span[0] = _value;
        ULog.Encoding.GetBytes(span, buffer);
        buffer = buffer[1..];
    }

    public override int GetByteSize() => sizeof(byte);
    internal override ValueType GetValue() => _value;
    
}