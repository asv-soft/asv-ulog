using Asv.IO;

namespace Asv.ULog;

/// <summary>
/// 'L': Logged String Message
/// 
/// Logged string message, i.e. printf() output.
/// </summary>
public class ULogLoggedStringMessageToken : IULogDataToken, IEquatable<ULogLoggedStringMessageToken>
{
    #region Static

    public static ULogToken Token => ULogToken.LoggedString;
    public const string Name = "Logged String message";
    public const byte TokenId = (byte)'L';

    #endregion

    private string _messageName = null!;

    public string TokenName => Name;
    public ULogToken TokenType => Token;
    public UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Data;

    /// <summary>
    /// log level same as in the Linux kernel
    /// </summary>
    public ULogLevel LogLevel { get; set; }

    /// <summary>
    /// TimeStamp value in microseconds
    /// </summary>
    public ulong TimeStamp { get; set; }

    /// <summary>
    /// Message name
    /// </summary>
    public string Message
    {
        get => _messageName;
        set
        {
            ULogFormatMessageToken.CheckMessageName(value);
            _messageName = value;
        }
    }


    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        var level = BinSerialize.ReadByte(ref buffer);
        LogLevel = (ULogLevel)level;

        TimeStamp = BinSerialize.ReadULong(ref buffer);

        var messageBytes = buffer[..buffer.Length];
        Message = ULog.Encoding.GetString(messageBytes);
        buffer = buffer[Message.Length..];
    }

    public void Serialize(ref Span<byte> buffer)
    {
        ULogFormatMessageToken.CheckMessageName(Message);
        BinSerialize.WriteByte(ref buffer, (byte)LogLevel);
        BinSerialize.WriteULong(ref buffer, TimeStamp);
        Message.CopyTo(ref buffer, ULog.Encoding);
    }

    public int GetByteSize()
    {
        return sizeof(byte) /*LogLevel*/
               + sizeof(ulong) /*Timestamp*/
               + ULog.Encoding.GetByteCount(Message);
    }

    public enum ULogLevel
    {
        /// <summary>
        /// System is unusable
        /// </summary>
        Emerg = '0',

        /// <summary>
        /// Action must be taken immediately
        /// </summary>
        Alert = '1',

        /// <summary>
        /// Critical conditions
        /// </summary>
        Crit = '2',

        /// <summary>
        /// Error conditions
        /// </summary>
        Err = '3',

        /// <summary>
        /// Warning conditions
        /// </summary>
        Warning = '4',

        /// <summary>
        /// Normal but significant condition
        /// </summary>
        Notice = '5',

        /// <summary>
        /// Informational
        /// </summary>
        Info = '6',

        /// <summary>
        /// Debug-level messages
        /// </summary>
        Debug = '7'
    }

    public bool Equals(ULogLoggedStringMessageToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Message == other.Message && LogLevel == other.LogLevel && TimeStamp == other.TimeStamp;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogLoggedStringMessageToken)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Message, (int)LogLevel, TimeStamp);
    }
}