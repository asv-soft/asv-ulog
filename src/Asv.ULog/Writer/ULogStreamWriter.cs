using System.Buffers;
using Asv.IO;

namespace Asv.ULog;

public class ULogStreamWriter : ULogWriter
{
    private readonly Stream _stream;

    public ULogStreamWriter(Stream stream, string sourceName, int? writeSyncTokenEveryXToken = null, bool disposeStream = false) 
        : base(sourceName, writeSyncTokenEveryXToken, disposeStream ? stream : null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _stream = stream;
    }

    protected override void InternalAppendHeader(ULogFileHeaderToken token)
    {
        var size = token.GetByteSize();
        var buffer = ArrayPool<byte>.Shared.Rent(size);
        try
        {
            var span = new Span<byte>(buffer, 0, size);
            token.Serialize(ref span);
            _stream.Write(buffer, 0, size);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    protected override void InternalAppend(IULogToken token)
    {
        var size = token.GetByteSize();
        var totalSize = size + ULog.TokenHeaderSize;
        var buffer = ArrayPool<byte>.Shared.Rent(totalSize);
        try
        {
            var span = new Span<byte>(buffer, 0, totalSize);
            BinSerialize.WriteUShort(ref span, (ushort)size);
            BinSerialize.WriteByte(ref span, (byte)token.TokenType);
            token.Serialize(ref span);
            _stream.Write(buffer, 0, totalSize);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}