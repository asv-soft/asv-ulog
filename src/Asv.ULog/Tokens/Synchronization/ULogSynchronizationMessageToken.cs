namespace Asv.ULog;

/// <summary>
/// 'S': Synchronization message.
/// 
/// Message so that a reader can recover from a corrupt message by searching for the next sync message.
/// </summary>
public class ULogSynchronizationMessageToken : IULogDataToken, IEquatable<ULogSynchronizationMessageToken>
{
    #region Static

    public static ULogToken Type => ULogToken.Synchronization;
    public const string Name = "Synchronization";
    public const byte TokenId = (byte)'S';
    public static ULogSynchronizationMessageToken Instance { get; } = new();

    #endregion

    public string TokenName => Name;
    public ULogToken TokenType => Type;
    public UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Data;
    

    /// <summary>
    /// Magic byte sequence for synchronization
    /// </summary>
    public static byte[] SyncMagic { get; } = [0x2F, 0x73, 0x13, 0x20, 0x25, 0x0C, 0xBB, 0x12];

    /// <summary>
    /// Full message with header.
    /// Not part of a real token! 
    /// </summary>
    public static byte[] FullMessage { get; } = [0x08, 0x00, TokenId, 0x2F, 0x73, 0x13, 0x20, 0x25, 0x0C, 0xBB, 0x12];

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        for (var i = 0; i < SyncMagic.Length; i++)
            if (buffer[i] != SyncMagic[i])
                throw new ULogException(
                    $"Error to parse Sync message: SyncMagic[{i}] want{SyncMagic[i]}. Got {buffer[i]}");

        buffer = buffer[SyncMagic.Length..];
    }

    public void Serialize(ref Span<byte> buffer)
    {
        SyncMagic.CopyTo(buffer);
        buffer = buffer[SyncMagic.Length..];
        
    }

    public int GetByteSize()
    {
        return SyncMagic.Length;
    }

    public bool Equals(ULogSynchronizationMessageToken? other) => true;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogSynchronizationMessageToken)obj);
    }

    public override int GetHashCode() => base.GetHashCode();
}