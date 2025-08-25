using System.Buffers;
using Asv.IO;

namespace Asv.ULog;

public class ULogBufferTokenWriter : ULogTokenWriter
{
    private readonly IBufferWriter<byte> _buffer;

    public ULogBufferTokenWriter(IBufferWriter<byte> buffer, string sourceName, int? writeSyncTokenEveryXToken, bool disposeBuffer = true) 
        : base(sourceName, writeSyncTokenEveryXToken, disposeBuffer ? buffer as IDisposable : null)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _buffer = buffer;
    }

    protected override void InternalAppendHeader(ULogFileHeaderToken token)
    {
        var size = token.GetByteSize();
        var span = _buffer.GetSpan(size);
        token.Serialize(ref span);
        _buffer.Advance(size);
    }

    protected override void InternalAppend(IULogToken token)
    {
        var size = token.GetByteSize();
        var totalSize = size + ULogManager.TokenHeaderSize;
        
        var span = _buffer.GetSpan(totalSize);
        BinSerialize.WriteUShort(ref span, (ushort)size);
        BinSerialize.WriteByte(ref span, (byte)token.TokenType);
        token.Serialize(ref span);
        _buffer.Advance(totalSize);
    }
}