using System.Buffers;
using System.Collections.Immutable;
using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.ULog;

public interface IULogWriter
{
    bool TryInit(IBufferWriter<byte> bufferWriter, ULogFileHeaderToken header, ULogFlagBitsMessageToken flagBits, 
        IImmutableList<IULogToken> definitionTokens);
    bool TryAppend<TToken>(IBufferWriter<byte> bufferWriter, TToken token) where TToken : class, IULogToken;
}

public class ULogWriter(ILogger? logger = null) : IULogWriter
{
    private ULogWritingState _state = ULogWritingState.EmptyFile;

    public bool TryInit(IBufferWriter<byte> bufferWriter, ULogFileHeaderToken header, ULogFlagBitsMessageToken flagBits, 
        IImmutableList<IULogToken> definitionTokens)
    {
        // Write Header
        var result = TryAppend(bufferWriter, header);
        if (!result) throw new ULogException("Failed to Write ULogFileHeaderToken");

        // Write FlagBits
        result = TryAppend(bufferWriter, flagBits);
        if (!result) throw new ULogException("Failed to Write ULogFlagBitsMessageToken");

        _state = ULogWritingState.DefinitionWriting;
        
        // Write Definitions
        foreach (var token in definitionTokens)
        {
            result = TryAppend(bufferWriter, token);
            if (!result) throw new ULogException($"Failed to Write a definition token: {token.GetType()}");
        }

        return true;
    }

    public bool TryAppend<TToken>(IBufferWriter<byte> bufferWriter, TToken token) where TToken : class, IULogToken
    {
        switch (_state)
        {
            case ULogWritingState.EmptyFile:
                if (token is ULogFileHeaderToken)
                {
                    _state = ULogWritingState.DefinitionWriting;
                }
                else
                {
                    return false;
                }
                break;
            
            case ULogWritingState.DefinitionWriting:
                if (!token.TokenSection.HasFlag(TokenPlaceFlags.Definition))
                {
                    if (token.TokenSection != TokenPlaceFlags.Data)
                        return false;
                    _state = ULogWritingState.DataWriting;
                }
                break;

            case ULogWritingState.DataWriting:
                if (!token.TokenSection.HasFlag(TokenPlaceFlags.Data))
                {
                    return false;
                }
                break;

            default:
                throw new ULogException("Unexpected state in ULogWriter.");
        }

        var size = token.GetByteSize();
        if (token is ULogFileHeaderToken header)
        {
            var span = bufferWriter.GetSpan(size);
            header.Serialize(ref span);
            bufferWriter.Advance(size);
        }
        else
        {
            const int tokenHeader = sizeof(ushort) + sizeof(byte);
            var span = bufferWriter.GetSpan(size + tokenHeader);
            BinSerialize.WriteUShort(ref span, (ushort)size);
            BinSerialize.WriteByte(ref span, (byte)token.TokenType);
            token.Serialize(ref span);
            bufferWriter.Advance(size + tokenHeader);
        }

        return true;
    }

    private enum ULogWritingState
    {
        EmptyFile,
        DefinitionWriting,
        DataWriting
    }
}
