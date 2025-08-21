using System.Buffers;
using Asv.Common;
using Asv.IO;
using R3;

namespace Asv.ULog;

public class ULogWriter : AsyncDisposableOnce, IULogWriter, IULogWriter.IDefinitionSection, IULogWriter.IDataSection
{
    private readonly IULogTokenWriter _writer;
    private readonly IDisposable _disposeIt;
    private readonly Lock _sync = new();

    public ULogWriter(IULogTokenWriter writer, DateTime timestamp, bool leaveOpen = false)
    {
        _writer = writer;
        var builder = Disposable.CreateBuilder();
        if (leaveOpen == false)
        {
            writer.AddTo(ref builder);
        }
        
        _disposeIt = builder.Build();
        
        var header = new ULogFileHeaderToken
        {
            Version = 1,
            Timestamp = ULogManager.FromDateTimeToUnixMicroseconds(timestamp),
        };
        var flags = new ULogFlagBitsMessageToken
        {
            AppendedOffsets = [],
            CompatFlags = [],
            IncompatFlags = []
        };
        _writer.AppendHeader(header, flags);
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
        using(_sync.EnterScope())
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
        using(_sync.EnterScope())
        {
            _cachedDefaultParamFloatToken.DefaultType = type;
            _cachedDefaultParamFloatToken.Key.Name = name;
            _cachedDefaultParamFloatToken.Value = value;
            _writer.AppendDefinition(_cachedDefaultParamFloatToken);
        }
    }

    public void DefineFormat(ULogFormatMessageToken token)
    {
        using(_sync.EnterScope())
        {
            _writer.AppendDefinition(token);
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
        using(_sync.EnterScope())
        {
            _cachedParamIntToken.Key.Name = name;
            _cachedParamIntToken.Value = value;
            _writer.AppendData(_cachedParamIntToken);
        }
    }
    private readonly ULogParameterMessageToken _cachedParamFloatToken = new()
    {
        Key = new ULogTypeAndNameDefinition
        {
            Type = SimpleFloatType
        },
    };

    private readonly Dictionary<SubscriptionKey,ushort> _subscriptions = new();

    public void WriteParameter(string name, float value)
    {
        using(_sync.EnterScope())
        {
            _cachedParamFloatToken.Key.Name = name;
            _cachedParamFloatToken.Value = value;
            _writer.AppendData(_cachedParamFloatToken);
        }
    }

    public void WriteSubscription(string messageName, byte multiId)
    {
        using (_sync.EnterScope())
        {
            var messageId = (ushort)_subscriptions.Count;
            _subscriptions.Add(new SubscriptionKey(messageName, multiId), messageId);
            _writer.AppendData(new ULogSubscriptionMessageToken
            {
                MultiId = multiId,
                MessageId = messageId,
                MessageName = messageName,
            });
        }
    }

    public void WriteData(string messageName, byte multiId, ISizedSpanSerializable data)
    {
        using (_sync.EnterScope())
        {
            var key = new SubscriptionKey(messageName, multiId);
            if (!_subscriptions.TryGetValue(key, out var messageId))
            {
                throw new ULogException($"Subscription for message {messageName} not found");
            }
            var size = data.GetByteSize();
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                data.Serialize(buffer,0, size);
                _writer.AppendData(new ULogLoggedDataMessageToken
                {
                    MessageId = messageId,
                    Data = new ArraySegment<byte>(buffer,0, size)
                });
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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


    public IULogWriter.IDefinitionSection Definition => this;
    public IULogWriter.IDataSection Data => this;
}

public record SubscriptionKey(string MessageName, byte MultiId);