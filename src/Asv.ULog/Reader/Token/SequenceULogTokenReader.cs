using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.ULog;

public class SequenceULogTokenReader : IULogTokenReader, IDisposable, IAsyncDisposable
{
    private readonly ImmutableDictionary<byte, Func<IULogToken>> _factory;
    private readonly ILogger _logger;

    private readonly ReadOnlySequence<byte> _sequence;
    private SequencePosition _position;
    private ReaderState _state = ReaderState.HeaderSection;

    // Threshold for stackalloc; above this size we rent from ArrayPool
    private const int StackallocThreshold = 1024;

    public SequenceULogTokenReader(ref SequenceReader<byte> rdr,
                                   ImmutableDictionary<byte, Func<IULogToken>>? factory = null,
                                   ILogger? logger = null)
    {
        _sequence = rdr.Sequence;
        _position = rdr.Position;
        _factory  = factory ?? ULogManager.TokenFactory;
        _logger   = logger  ?? NullLogger.Instance;
    }

    public bool TryRead(out IULogToken? token)
    {
        token = null;

        while (true)
        {
            switch (_state)
            {
                case ReaderState.HeaderSection:
                    if (!TryReadHeader(out token)) return false;
                    _state = ReaderState.FlagBitsMessage;
                    return true;

                case ReaderState.FlagBitsMessage:
                    if (!TryReadToken(out token)) return false;
                    Debug.Assert(token != null);
                    if (token.TokenType != ULogToken.FlagBits)
                        throw new ULogException($"{ULogToken.FlagBits:G} must follow header, but got {token.TokenType:G}");
                    _state = ReaderState.DefinitionSection;
                    return true;

                case ReaderState.DefinitionSection:
                    if (!TryReadToken(out token)) return false;
                    Debug.Assert(token != null);
                    // switch to data section once we see a token that is only allowed in Data
                    if (token.TokenSection.HasFlag(UTokenPlaceFlags.Data) &&
                        !token.TokenSection.HasFlag(UTokenPlaceFlags.Definition))
                    {
                        _state = ReaderState.DataSection;
                    }
                    return true;

                case ReaderState.DataSection:
                    try
                    {
                        if (!TryReadToken(out token)) return false;
                        Debug.Assert(token != null);

                        if (token.TokenType is ULogToken.Unknown)
                            throw new ULogUnknownTokenException();

                        if (!token.TokenSection.HasFlag(UTokenPlaceFlags.Data))
                            throw new ULogWrongTokenSectionException();

                        return true;
                    }
                    catch (ULogException)
                    {
                        _state = ReaderState.Corrupted;
                        continue; // go search for sync sequence
                    }

                case ReaderState.Corrupted:
                    if (!TryReadSyncSequence(out token)) return false;
                    _state = ReaderState.DataSection;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    #region Core readers

    private bool TryReadHeader(out IULogToken? token)
    {
        token = null;

        var r = new SequenceReader<byte>(_sequence.Slice(_position));
        int need = ULogFileHeaderToken.HeaderSize;
        if (r.Remaining < need) return false;

        // Fast path: contiguous span is large enough
        if (r.UnreadSpan.Length >= need)
        {
            ReadOnlySpan<byte> span = r.UnreadSpan.Slice(0, need);
            var header = new ULogFileHeaderToken();
            header.Deserialize(ref span);
            r.Advance(need);
            _position = r.Position;
            token = header;
            return true;
        }

        // Non-contiguous: copy to stack or pooled array and deserialize within scope
        if (need <= StackallocThreshold)
        {
            Span<byte> tmp = stackalloc byte[need];
            if (!r.TryCopyTo(tmp)) return false;
            var span = (ReadOnlySpan<byte>)tmp;
            var header = new ULogFileHeaderToken();
            header.Deserialize(ref span); // deserialize while tmp is in scope
            r.Advance(need);
            _position = r.Position;
            token = header;
            return true;
        }
        else
        {
            var rented = ArrayPool<byte>.Shared.Rent(need);
            try
            {
                Span<byte> tmp = rented.AsSpan(0, need);
                if (!r.TryCopyTo(tmp)) return false;
                var span = (ReadOnlySpan<byte>)tmp;
                var header = new ULogFileHeaderToken();
                header.Deserialize(ref span); // deserialize while rented buffer is in scope
                r.Advance(need);
                _position = r.Position;
                token = header;
                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private bool TryReadToken(out IULogToken? token)
    {
        token = null;
        var r = new SequenceReader<byte>(_sequence.Slice(_position));

        // size (2 LE) + type (1)
        if (!r.TryReadLittleEndian(out ushort size)) return false;
        if (!r.TryRead(out byte type)) return false;

        if (r.Remaining < size) return false; // not enough payload yet

        // Contiguous payload: zero-copy
        if (r.UnreadSpan.Length >= size)
        {
            ReadOnlySpan<byte> payload = r.UnreadSpan.Slice(0, size);
            var inst = _factory.TryGetValue(type, out var f) ? f() : new ULogUnknownToken(type, size);
            inst.Deserialize(ref payload); // use directly
            r.Advance(size);
            _position = r.Position;
            token = inst;
            return true;
        }

        // Non-contiguous: copy to stack or pooled array, deserialize inside the scope
        if (size <= StackallocThreshold)
        {
            Span<byte> tmp = stackalloc byte[size];
            if (!r.TryCopyTo(tmp)) return false;
            ReadOnlySpan<byte> payload = tmp;
            var inst = _factory.TryGetValue(type, out var f) ? f() : new ULogUnknownToken(type, size);
            inst.Deserialize(ref payload); // deserialize while tmp is in scope
            r.Advance(size);
            _position = r.Position;
            token = inst;
            return true;
        }
        else
        {
            byte[] rented = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                Span<byte> tmp = rented.AsSpan(0, size);
                if (!r.TryCopyTo(tmp)) return false;
                ReadOnlySpan<byte> payload = tmp;
                var inst = _factory.TryGetValue(type, out var f) ? f() : new ULogUnknownToken(type, size);
                inst.Deserialize(ref payload); // deserialize while rented buffer is in scope
                r.Advance(size);
                _position = r.Position;
                token = inst;
                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private bool TryReadSyncSequence(out IULogToken? token)
    {
        token = null;
        ReadOnlySpan<byte> sync = ULogSynchronizationMessageToken.FullMessage;
        var r = new SequenceReader<byte>(_sequence.Slice(_position));

        if (r.Remaining < sync.Length) return false;

        int matched = 0;
        while (!r.End)
        {
            if (!r.TryRead(out byte b)) break;

            if (b == sync[matched])
            {
                matched++;
                if (matched == sync.Length)
                {
                    // Found the whole sync pattern; position already advanced past it
                    _position = r.Position;
                    token = new ULogSynchronizationMessageToken();
                    return true;
                }
            }
            else
            {
                matched = (b == sync[0]) ? 1 : 0;
            }
        }

        return false;
    }

    #endregion

    public void Dispose()
    {
        // Nothing to dispose: the sequence owner is external
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private enum ReaderState
    {
        HeaderSection,
        FlagBitsMessage,
        DataSection,
        Corrupted,
        DefinitionSection,
    }
}
