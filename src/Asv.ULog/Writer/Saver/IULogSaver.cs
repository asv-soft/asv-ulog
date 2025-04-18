using Asv.Common;
using R3;

namespace Asv.ULog;

public interface IULogSaver
{
    void Start(DateTime timestamp);
    void DefineParameter(string name, int value, ULogParameterDefaultTypes type);
    void DefineParameter(string name, float value, ULogParameterDefaultTypes type);
    void WriteParameter(string name, int value);
    void WriteParameter(string name, float value);
}

public class ULogSaver : AsyncDisposableOnce, IULogSaver
{
    private readonly IULogWriter _writer;
    private readonly IDisposable _disposeIt;
    private readonly object _sync = new();

    public ULogSaver(IULogWriter writer, bool leaveOpen = false)
    {
        _writer = writer;
        var builder = Disposable.CreateBuilder();
        if (leaveOpen == false)
        {
            writer.AddTo(ref builder);
        }
        
        
        _disposeIt = builder.Build();
        
    }
    
    public void Start(DateTime timestamp)
    {
        var header = new ULogFileHeaderToken
        {
            Version = 1,
            Timestamp = ULog.FromDateTimeToUnixMicroseconds(timestamp),
        };
        var flags = new ULogFlagBitsMessageToken
        {
            AppendedOffsets = [],
            CompatFlags = [],
            IncompatFlags = []
        };
        lock (_sync)
        {
            _writer.AppendHeader(header, flags);    
        }
    }

    #region Simple types !!! READ ONLY !!!

    private static readonly ULogTypeDefinition SimpleFloatType = new()
    {
        BaseType = ULogType.Float,
        TypeName = ULogTypeDefinition.FloatTypeName,
        ArraySize = 0,
    };
    private static readonly ULogTypeDefinition SimpleInt32Type = new()
    {
        BaseType = ULogType.Int32,
        TypeName = ULogTypeDefinition.Int32TypeName,
        ArraySize = 0,
    };

    #endregion
    
    #region 'Q': Default Parameter Message

    
    
    private readonly ULogDefaultParameterMessageToken _cachedDefaultParamIntToken = new()
    {
        Key = new ULogTypeAndNameDefinition
        {
            Type = SimpleInt32Type
        },
    };
    
    public void DefineParameter(string name, int value, ULogParameterDefaultTypes type)
    {
        lock (_sync)
        {
            _cachedDefaultParamIntToken.DefaultType = type;
            _cachedDefaultParamIntToken.Key.Name = name;
            _cachedDefaultParamIntToken.Value = value;
            _writer.AppendDefinition(_cachedDefaultParamIntToken);
        }
    }
    
    private readonly ULogDefaultParameterMessageToken _cachedDefaultParamFloatToken = new()
    {
        Key = new ULogTypeAndNameDefinition
        {
            Type = SimpleFloatType
        },
    };

    
    public void DefineParameter(string name, float value, ULogParameterDefaultTypes type)
    {
        lock (_sync)
        {
            _cachedDefaultParamFloatToken.DefaultType = type;
            _cachedDefaultParamFloatToken.Key.Name = name;
            _cachedDefaultParamFloatToken.Value = value;
            _writer.AppendDefinition(_cachedDefaultParamFloatToken);
        }
    }

    #endregion
    
    #region 'P': Parameter Message
    
    private readonly ULogParameterMessageToken _cachedParamIntToken = new()
    {
        Key = new ULogTypeAndNameDefinition
        {
            Type = SimpleInt32Type
        },
    };
    

    
    public void WriteParameter(string name, int value)
    {
        lock (_sync)
        {
            _cachedParamIntToken.Key.Name = name;
            _cachedParamIntToken.Value = value;
            _writer.AppendDefinition(_cachedParamIntToken);
        }
    }
    private readonly ULogParameterMessageToken _cachedParamFloatToken = new()
    {
        Key = new ULogTypeAndNameDefinition
        {
            Type = SimpleFloatType
        },
    };
    public void WriteParameter(string name, float value)
    {
        lock (_sync)
        {
            _cachedParamFloatToken.Key.Name = name;
            _cachedParamFloatToken.Value = value;
            _writer.AppendDefinition(_cachedParamFloatToken);
        }
    }
    

    #endregion

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposeIt.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_disposeIt is IAsyncDisposable disposeItAsyncDisposable)
            await disposeItAsyncDisposable.DisposeAsync();
        else
            _disposeIt.Dispose();

        await base.DisposeAsyncCore();
    }

    #endregion

    
}