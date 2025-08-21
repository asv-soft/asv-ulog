using Asv.IO;

namespace Asv.ULog;

public class ULogTaggedLoggedStringMessageToken : IULogDataToken, IEquatable<ULogTaggedLoggedStringMessageToken>
{
    #region Static

    public static ULogToken Token => ULogToken.TaggedLoggedString;
    public static readonly string Name = "TaggedLoggedString";
    public static readonly byte TokenId = (byte)'C';

    #endregion

    public string TokenName => Name;
    public ULogToken TokenType => Token;
    public UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Data;

    /// <summary>
    /// log_level: same as in the Linux kernel
    /// </summary>
    public ULogLevel LogLevel { get; set; }

    /// <summary>
    /// tag: id representing source of logged message string. It could represent a process, thread or a class depending upon the system architecture.
    /// </summary>
    public ushort Tag { get; set; }

    /// <summary>
    /// timestamp: in microseconds
    /// </summary>
    public ulong Timestamp { get; set; }

    /// <summary>
    /// Logged string message, i.e. printf() output.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        var level = BinSerialize.ReadByte(ref buffer);
        LogLevel = (ULogLevel)level;

        var tag = BinSerialize.ReadUShort(ref buffer);
        Tag = tag;

        Timestamp = BinSerialize.ReadULong(ref buffer);

        Message = ULogManager.Encoding.GetString(buffer);
    }

    public void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteByte(ref buffer, (byte)LogLevel);
        BinSerialize.WriteUShort(ref buffer, Tag);
        BinSerialize.WriteULong(ref buffer, Timestamp);
        Message.CopyTo(ref buffer, ULogManager.Encoding);
    }

    public int GetByteSize()
    {
        return sizeof(ULogLevel) /*LogLevel*/
               + sizeof(ushort) /*Tag*/
               + sizeof(ulong) /*Timestamp*/
               + ULogManager.Encoding.GetByteCount(Message);
    }

    public enum ULogLevel : byte
    {
        /// <summary>
        /// System is unusable
        /// </summary>
        Emerg = 0,

        /// <summary>
        /// Action must be taken immediately
        /// </summary>
        Alert = 1,

        /// <summary>
        /// Critical conditions
        /// </summary>
        Crit = 2,

        /// <summary>
        /// Error conditions
        /// </summary>
        Err = 3,

        /// <summary>
        /// Warning conditions
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Normal but significant condition
        /// </summary>
        Notice = 5,

        /// <summary>
        /// Informational
        /// </summary>
        Info = 6,

        /// <summary>
        /// Debug-level messages
        /// </summary>
        Debug = 7
    }

    public bool Equals(ULogTaggedLoggedStringMessageToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return LogLevel == other.LogLevel && Tag == other.Tag && Timestamp == other.Timestamp && Message == other.Message;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogTaggedLoggedStringMessageToken)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)LogLevel, Tag, Timestamp, Message);
    }
}