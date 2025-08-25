using Asv.Common;

namespace Asv.ULog;

public abstract class ULogTokenWriter : AsyncDisposableOnce, IULogTokenWriter
{
    private ULogWriterState _logWriterState;
    private readonly int? _writeSyncTokenEveryXToken;
    private readonly IDisposable? _disposeIt;
    private int _dataTokenCount;

    protected ULogTokenWriter(string sourceName, int? writeSyncTokenEveryXToken, IDisposable? disposeIt)
    {
        ArgumentNullException.ThrowIfNull(sourceName);
        SourceName = sourceName;
        _logWriterState = ULogWriterState.AppendHeader;
        _writeSyncTokenEveryXToken = writeSyncTokenEveryXToken;
        _disposeIt = disposeIt;
    }

    public string SourceName { get; }
    
    public IULogTokenWriter AppendHeader(ULogFileHeaderToken header, ULogFlagBitsMessageToken flags)
    {
        if (_logWriterState != ULogWriterState.AppendHeader)
        {
            throw new ULogWriterException($"ULog header already written. Current state: {_logWriterState:G}");
        }
        InternalAppendHeader(header);
        InternalAppend(flags);
        _logWriterState = ULogWriterState.AppendDefinition;
        return this;
    }

    public IULogTokenWriter AppendDefinition(IULogDefinitionToken definitionToken)
    {
        switch (_logWriterState)
        {
            case ULogWriterState.AppendHeader:
                throw new ULogWriterException($"Can't {nameof(AppendDefinition)}: you must {nameof(AppendHeader)} first. Current state: {_logWriterState}");
            case > ULogWriterState.AppendDefinition:
                throw new ULogException($"Can't {nameof(AppendDefinition)}: definition already written. You must {nameof(ULogWriterState.AppendDefinition)} before {_logWriterState:G}");
            default:
                if (definitionToken.TokenSection.HasFlag(UTokenPlaceFlags.Definition) == false)
                {
                    throw new ULogException($"Can't {nameof(AppendDefinition)}: token {definitionToken.TokenType} is not a definition. You must use {nameof(ULogToken)} with {nameof(UTokenPlaceFlags.Definition)} flag.");
                }
                InternalAppend(definitionToken);
                break;
        }

        return this;
    }

    public IULogTokenWriter AppendData(IULogDataToken dataToken)
    {
        if (_logWriterState == ULogWriterState.AppendHeader)
        {
            throw new ULogWriterException($"Can't {nameof(AppendData)}: you must {nameof(AppendHeader)} and {nameof(AppendDefinition)} first. Current state: {_logWriterState}");
        }
        if (dataToken.TokenSection.HasFlag(UTokenPlaceFlags.Data) == false)
        {
            throw new ULogException($"Can't {nameof(AppendData)}: token {dataToken.TokenType} is not a definition. You must use {nameof(ULogToken)} with {nameof(UTokenPlaceFlags.Data)} flag.");
        }
        _logWriterState = ULogWriterState.AppendData;
        InternalAppend(dataToken);
        _dataTokenCount++;
        if (_writeSyncTokenEveryXToken != null && _dataTokenCount % _writeSyncTokenEveryXToken == 0)
        {
            // Write sync token
            InternalAppend(ULogSynchronizationMessageToken.Instance);
        }
        return this;
    }

    protected abstract void InternalAppendHeader(ULogFileHeaderToken header);
    protected abstract void InternalAppend(IULogToken token);
    
    
    private enum ULogWriterState
    {
        AppendHeader,
        AppendDefinition,
        AppendData,
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposeIt?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_disposeIt is IAsyncDisposable disposeItAsyncDisposable)
            await disposeItAsyncDisposable.DisposeAsync();
        else if (_disposeIt != null)
            _disposeIt.Dispose();

        await base.DisposeAsyncCore();
    }
}
