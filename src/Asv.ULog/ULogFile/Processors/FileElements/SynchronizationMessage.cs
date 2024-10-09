namespace Asv.ULog.ULogFile.Processors;

public sealed class SynchronizationMessage
{
    public static byte[] SyncMagic { get; } = ULogSynchronizationMessageToken.SyncMagic;
}