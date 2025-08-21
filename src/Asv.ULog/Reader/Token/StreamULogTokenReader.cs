using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.ULog;

public class StreamULogTokenReader : IULogTokenReader, IDisposable, IAsyncDisposable
{
    private const int DefaultBufferSize = 64 * 1024;

    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly ILogger _logger;
    private readonly ImmutableDictionary<byte, Func<IULogToken>> _factory;

    private byte[] _buffer;
    private int _start; // index of the window start
    private int _end;   // index of the window end (exclusive)

    private ReaderState _state = ReaderState.HeaderSection;
    private bool _eof;

    public StreamULogTokenReader(
        Stream stream,
        int initialBufferSize = DefaultBufferSize,
        ImmutableDictionary<byte, Func<IULogToken>>? factory = null,
        bool leaveOpen = false,
        ILogger? logger = null)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead) throw new ArgumentException("Stream must be readable", nameof(stream));
        _leaveOpen = leaveOpen;
        _logger = logger ?? NullLogger.Instance;
        _factory = factory ?? ULogManager.TokenFactory;

        if (initialBufferSize < 256) initialBufferSize = 256;
        _buffer = ArrayPool<byte>.Shared.Rent(initialBufferSize);
        _start = 0;
        _end = 0;
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
                        continue;
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
        var need = ULogFileHeaderToken.HeaderSize;
        if (!EnsureAvailable(need)) return false;

        var span = new ReadOnlySpan<byte>(_buffer, _start, need);
        var header = new ULogFileHeaderToken();
        header.Deserialize(ref span);
        Advance(need);
        token = header;
        return true;
    }

    private bool TryReadToken(out IULogToken? token)
    {
        token = null;

        // Need at least 3 bytes: size(2, LE) + type(1)
        if (!EnsureAvailable(3)) return false;

        var size = ReadUInt16LE();
        var type = ReadByte();

        // Token payload
        if (!EnsureAvailable(size))
        {
            // rollback token header (size+type) to retry later
            Rewind(3);
            return false;
        }

        var payload = new ReadOnlySpan<byte>(_buffer, _start, size);

        var inst = _factory.TryGetValue(type, out var f) ? f() : new ULogUnknownToken(type, size);
        var tmp = payload; // ref аргумент
        inst.Deserialize(ref tmp);

        Advance(size);
        token = inst;
        return true;
    }

    private bool TryReadSyncSequence(out IULogToken? token)
    {
        token = null;
        ReadOnlySpan<byte> sync = ULogSynchronizationMessageToken.FullMessage;

        if (!EnsureAvailable(sync.Length)) return false;

        var i = 0;
        var cursor = _start;

        while (cursor < _end)
        {
            var b = _buffer[cursor++];

            if (b == sync[i])
            {
                i++;
                if (i == sync.Length)
                {
                    // потребляем ровно sync.Length байт
                    Advance(cursor - _start - (i - sync.Length));
                    token = new ULogSynchronizationMessageToken();
                    return true;
                }
            }
            else
            {
                i = (b == sync[0]) ? 1 : 0;
            }

            if (cursor >= _end)
            {
                // сохраняем хвост возможного префикса
                var keep = Math.Min(sync.Length - 1, _end - _start);
                if (!EnsureAvailable(keep + 1))
                {
                    // оставим в буфере последние keep байт
                    var avail = Available;
                    if (avail > keep) Advance(avail - keep);
                    return false;
                }
                cursor = _start + i;
            }
        }

        return false;
    }

    #endregion

    #region Buffer helpers

    private int Available => _end - _start;

    private bool EnsureAvailable(int count)
    {
        while (Available < count)
        {
            if (_eof) return false;

            CompactIfNeeded(count);

            var free = _buffer.Length - _end;
            if (free == 0)
            {
                Grow(count);
                free = _buffer.Length - _end;
            }

            var read = _stream.Read(_buffer, _end, free);
            if (read == 0)
            {
                _eof = true;
                return Available >= count;
            }
            _end += read;
        }
        return true;
    }

    private void CompactIfNeeded(int need)
    {
        var tail = _buffer.Length - _end;
        if (tail >= need) return;
        if (_start == 0) return;

        Buffer.BlockCopy(_buffer, _start, _buffer, 0, _end - _start);
        _end -= _start;
        _start = 0;
    }

    private void Grow(int ensureCount)
    {
        var have = Available;
        var needed = Math.Max(ensureCount, have);
        var newSize = _buffer.Length;

        while (newSize - have < needed)
        {
            var next = newSize << 1;
            if (next <= 0) throw new OutOfMemoryException("Buffer too large");
            newSize = next;
        }

        var newBuf = ArrayPool<byte>.Shared.Rent(newSize);
        Buffer.BlockCopy(_buffer, _start, newBuf, 0, have);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuf;
        _end = have;
        _start = 0;
    }

    private void Advance(int count)
    {
        _start += count;
        Debug.Assert(_start <= _end);
        if (_start == _end)
        {
            _start = 0;
            _end = 0;
        }
    }

    private void Rewind(int count)
    {
        _start -= count;
        if (_start < 0) throw new InvalidOperationException("Rewind underflow");
    }

    private byte ReadByte()
    {
        var b = _buffer[_start];
        _start++;
        return b;
    }

    private ushort ReadUInt16LE()
    {
        // EnsureAvailable(2) уже вызван
        var span = MemoryMarshal.CreateReadOnlySpan(ref _buffer[_start], 2);
        var v = BinaryPrimitives.ReadUInt16LittleEndian(span);
        _start += 2;
        return v;
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        var buf = _buffer;
        _buffer = [];
        if (buf.Length > 0) ArrayPool<byte>.Shared.Return(buf);
        if (!_leaveOpen)
            _stream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        var buf = _buffer;
        _buffer = [];
        if (buf.Length > 0) ArrayPool<byte>.Shared.Return(buf);
        if (!_leaveOpen)
            await _stream.DisposeAsync().ConfigureAwait(false);
    }

    #endregion

    private enum ReaderState
    {
        HeaderSection,
        FlagBitsMessage,
        DataSection,
        Corrupted,
        DefinitionSection,
    }
}