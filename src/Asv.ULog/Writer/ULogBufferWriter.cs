using System.Buffers;
using Asv.IO;

namespace Asv.ULog;

public class ULogBufferWriter : ULogWriter
{
    private readonly IBufferWriter<byte> _buffer;

    public ULogBufferWriter(IBufferWriter<byte> buffer, string sourceName, int? writeSyncTokenEveryXToken, bool disposeBuffer = true) 
        : base(sourceName, writeSyncTokenEveryXToken, disposeBuffer ? buffer as IDisposable : null)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _buffer = buffer;
    }

    protected override void InternalAppend(IULogToken token)
    {
        var size = token.GetByteSize();
        var totalSize = size + ULog.TokenHeaderSize;
        
        var span = _buffer.GetSpan(totalSize);
        BinSerialize.WriteUShort(ref span, (ushort)size);
        BinSerialize.WriteByte(ref span, (byte)token.TokenType);
        token.Serialize(ref span);
        _buffer.Advance(totalSize);
    }
}