using System.Buffers;
using Asv.IO;

namespace Asv.ULog;

/// <summary>
/// Header Section
/// The header is a fixed-size section and has the following format (16 bytes):
/// ----------------------------------------------------------------------
/// | 0x55 0x4c 0x6f 0x67 0x01 0x12 0x35 | 0x01         | uint64_t       |
/// | File magic (7B)                    | Version (1B) | Timestamp (8B) |
/// ----------------------------------------------------------------------
/// </summary>
public class ULogFileHeaderToken : IULogToken, IEquatable<ULogFileHeaderToken>
{
    #region Static

    /// <summary>
    /// File type indicator that reads "ULogXYZ where XYZ is the magic bytes sequence 0x01 0x12 0x35"
    /// </summary>
    private static readonly byte[] FileMagic = [0x55, 0x4c, 0x6f, 0x67, 0x01, 0x12, 0x35];

    public const int HeaderSize = 16;
    public const string Name = "FileHeader";
    public const ULogToken Type = ULogToken.FileHeader;

    #endregion

    private byte _version;
    private ulong _timestamp;


    /// <summary>
    /// File format version
    /// </summary>
    public byte Version
    {
        get => _version;
        set => _version = value;
    }

    /// <summary>
    /// uint64_t integer that denotes when the logging started in microseconds.
    /// </summary>
    public ulong Timestamp
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public string TokenName => Name;
    public ULogToken TokenType => Type;
    public UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Header;

    public bool TryRead(ReadOnlySequence<byte> data)
    {
        var rdr = new SequenceReader<byte>(data);
        for (var i = 0; i < FileMagic.Length; i++)
        {
            if (rdr.TryRead(out var b) == false)
            {
                rdr.Rewind(i);
                return false;
            }

            if (b != FileMagic[i])
            {
            }
        }

        if (rdr.TryRead(out _version) == false)
        {
            rdr.Rewind(FileMagic.Length);
            return false;
        }

        if (rdr.TryReadLittleEndian(out _timestamp) == false)
        {
            rdr.Rewind(FileMagic.Length + sizeof(byte)); // move back 7 (FileMagic) + 1 (version) bytes
            return false;
        }

        return true;
    }

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        for (var i = 0; i < FileMagic.Length; i++)
            if (buffer[i] != FileMagic[i])
                throw new ULogException(
                    $"Error to parse ULog header: FileMagic[{i}] want{FileMagic[i]}. Got {buffer[i]}");
        buffer = buffer[FileMagic.Length..];
        BinSerialize.ReadByte(ref buffer, ref _version);
        BinSerialize.ReadULong(ref buffer, ref _timestamp);
    }

    public void Serialize(ref Span<byte> buffer)
    {
        for (var i = 0; i < FileMagic.Length; i++)
        {
            buffer[i] = FileMagic[i];
        }
        buffer = buffer[FileMagic.Length..];
        BinSerialize.WriteByte(ref buffer, _version);
        BinSerialize.WriteULong(ref buffer, _timestamp);
    }

    public int GetByteSize()
    {
        return HeaderSize;
    }

    public override string ToString()
    {
        return $"Version:{Version},Timestamp:{Timestamp}";
    }

    public bool Equals(ULogFileHeaderToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Version == other.Version && Timestamp == other.Timestamp;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogFileHeaderToken)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Version, Timestamp);
    }
}