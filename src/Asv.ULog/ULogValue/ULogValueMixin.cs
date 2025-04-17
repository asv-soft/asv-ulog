namespace Asv.ULog;

public static class ULogValueMixin
{
    public static byte[] ToByteArray(this ULogValue src)
    {
        var result = new byte[src.GetByteSize()];
        var span = new Span<byte>(result);
        src.Serialize(ref span);
        return result;
    }
}