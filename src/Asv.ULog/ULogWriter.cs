using System.Buffers;
using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.ULog;

public interface IULogWriter
{
    void AddHeaderAndFlagBits(IBufferWriter<byte> bufferWriter, ULogFileHeaderToken header, ULogFlagBitsMessageToken flags);
    void AppendDefinition(IBufferWriter<byte> bufferWriter, IULogToken definitionToken);
    void AppendData(IBufferWriter<byte> bufferWriter, IULogToken dataToken);
}

public class ULogWriter(ILogger? logger = null) : IULogWriter
{
    private const int TokenHeaderSize = sizeof(ushort) + sizeof(byte);
    
    public void AddHeaderAndFlagBits(IBufferWriter<byte> bufferWriter, ULogFileHeaderToken header, ULogFlagBitsMessageToken flags)
    {
        var size = header.GetByteSize();
        var span = bufferWriter.GetSpan(size);
        header.Serialize(ref span);
        bufferWriter.Advance(size);
        
        size = flags.GetByteSize();
        span = bufferWriter.GetSpan(size + TokenHeaderSize);
        BinSerialize.WriteUShort(ref span, (ushort)size);
        BinSerialize.WriteByte(ref span, (byte)flags.TokenType);
        flags.Serialize(ref span);
        bufferWriter.Advance(size + TokenHeaderSize);
    }

    public void AppendDefinition(IBufferWriter<byte> bufferWriter, IULogToken definitionToken)
    {
        var size = definitionToken.GetByteSize();
        var span = bufferWriter.GetSpan(size + TokenHeaderSize);
        BinSerialize.WriteUShort(ref span, (ushort)size);
        BinSerialize.WriteByte(ref span, (byte)definitionToken.TokenType);
        definitionToken.Serialize(ref span);
        bufferWriter.Advance(size + TokenHeaderSize);
    }

    public void AppendData(IBufferWriter<byte> bufferWriter, IULogToken dataToken)
    {
        var size = dataToken.GetByteSize();
        var span = bufferWriter.GetSpan(size + TokenHeaderSize);
        BinSerialize.WriteUShort(ref span, (ushort)size);
        BinSerialize.WriteByte(ref span, (byte)dataToken.TokenType);
        dataToken.Serialize(ref span);
        bufferWriter.Advance(size + TokenHeaderSize);
    }
}
