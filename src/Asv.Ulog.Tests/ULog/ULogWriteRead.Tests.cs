using System.Buffers;
using System.Collections.Immutable;
using Asv.ULog;
using Asv.ULog.Information;
using Xunit.Abstractions;

namespace Asv.Ulog.Tests;

public class ULogWriteReadTests
{
    private readonly ITestOutputHelper _output;

    public ULogWriteReadTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void WriteReadFullULog_Success()
    {
        var writer = ULog.ULog.CreateWriter();
        var reader = ULog.ULog.CreateReader();

        var header = new ULogFileHeaderToken
        {
            Timestamp = 20309082UL,
            Version = 1
        };

        var fBits = new ULogFlagBitsMessageToken
        {
            CompatFlags = [1, 2, 3, 4, 5, 6, 7, 8],
            IncompatFlags = [1, 2, 3, 4, 5, 6, 7, 8],
            AppendedOffsets = [1UL, 2UL, 3UL]
        };

        var defs = new List<IULogToken>
        {
            new ULogFormatMessageToken
            {
                Fields =
                {
                    new ULogTypeAndNameDefinition
                    {
                        Name = "TestChar",
                        Type = new ULogTypeDefinition
                        {
                            ArraySize = 5,
                            BaseType = ULogType.Char,
                            TypeName = "char"
                        },
                    }
                },
                MessageName = "Test"
            },
            new ULogFormatMessageToken
            {
                Fields =
                {
                    new ULogTypeAndNameDefinition
                    {
                        Name = "Test",
                        Type = new ULogTypeDefinition
                        {
                            ArraySize = 0,
                            BaseType = ULogType.Int32,
                            TypeName = "int32"
                        },
                    }
                },
                MessageName = "Test2"
            }
        }.ToImmutableList();

        var infoToken = new ULogInformationMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "Test Info",
                Type = new ULogTypeDefinition
                {
                    ArraySize = 8,
                    BaseType = ULogType.Char,
                    TypeName = "char"
                },
            },
            Value = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        var paramToken = new ULogParameterMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "TestParam",
                Type = new ULogTypeDefinition
                {
                    ArraySize = 0,
                    BaseType = ULogType.Float,
                    TypeName = "float"
                }
            },
            Value = BitConverter.GetBytes(3.14f)
        };

        var loggedDataToken = new ULogLoggedDataMessageToken
        {
            MessageId = 1,
            Data = [10, 20, 30, 40]
        };

        var loggedStringToken = new ULogLoggedStringMessageToken
        {
            LogLevel = ULogLoggedStringMessageToken.ULogLevel.Info,
            TimeStamp = 12345678UL,
            Message = "This is a log message"
        };

        var syncToken = new ULogSynchronizationMessageToken();

        var dropoutToken = new ULogDropoutMessageToken
        {
            Duration = 100
        };

        var bufferWriter = new ArrayBufferWriter<byte>();

        var initResult = writer.TryInit(bufferWriter, header, fBits, defs);
        Assert.True(initResult, "Initialization failed");

        var appendResult = writer.TryAppend(bufferWriter, infoToken);
        Assert.True(appendResult, "Appending ULogInformationMessageToken failed");

        appendResult = writer.TryAppend(bufferWriter, paramToken);
        Assert.True(appendResult, "Appending ULogParameterMessageToken failed");

        appendResult = writer.TryAppend(bufferWriter, loggedDataToken);
        Assert.True(appendResult, "Appending ULogLoggedDataMessageToken failed");

        appendResult = writer.TryAppend(bufferWriter, loggedStringToken);
        Assert.True(appendResult, "Appending ULogLoggedStringMessageToken failed");

        appendResult = writer.TryAppend(bufferWriter, syncToken);
        Assert.True(appendResult, "Appending ULogSynchronizationMessageToken failed");

        appendResult = writer.TryAppend(bufferWriter, dropoutToken);
        Assert.True(appendResult, "Appending ULogDropoutMessageToken failed");

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        var tokenCount = 0;
        while (reader.TryRead(ref rdr, out var token))
        {
            tokenCount++;
            _output.WriteLine($"Read token: {token?.TokenName}");
        }

        Assert.Equal(10, tokenCount); 
    }

    [Fact]
    public void WriteReadFullLog_WrongOrder()
    {
        var writer = ULog.ULog.CreateWriter();
        var reader = ULog.ULog.CreateReader();
        
        var header = new ULogFileHeaderToken
        {
            Timestamp = 20309082UL,
            Version = 1
        };

        var fBits = new ULogFlagBitsMessageToken
        {
            CompatFlags = [1, 2, 3, 4, 5, 6, 7, 8],
            IncompatFlags = [1, 2, 3, 4, 5, 6, 7, 8],
            AppendedOffsets = [1UL, 2UL, 3UL]
        };
        
        var defs = new List<IULogToken>
        {
            new ULogFormatMessageToken
            {
                Fields =
                {
                    new ULogTypeAndNameDefinition
                    {
                        Name = "TestChar",
                        Type = new ULogTypeDefinition
                        {
                            ArraySize = 5,
                            BaseType = ULogType.Char,
                            TypeName = "char"
                        },
                    }
                },
                MessageName = "Test"
            },
            new ULogFormatMessageToken
            {
                Fields =
                {
                    new ULogTypeAndNameDefinition
                    {
                        Name = "Test",
                        Type = new ULogTypeDefinition
                        {
                            ArraySize = 0,
                            BaseType = ULogType.Int32,
                            TypeName = "int32"
                        },
                    }
                },
                MessageName = "Test2"
            }
        }.ToImmutableList();
        
        var loggedDataToken = new ULogLoggedDataMessageToken
        {
            MessageId = 1,
            Data = [10, 20, 30, 40]
        };

        var definitionFailedToken = new ULogFormatMessageToken
        {
            Fields =
            {
                new ULogTypeAndNameDefinition
                {
                    Name = "Test",
                    Type = new ULogTypeDefinition
                    {
                        ArraySize = 0,
                        BaseType = ULogType.Int32,
                        TypeName = "int32"
                    },
                }
            },
            MessageName = "Test2"
        };
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        var initResult = writer.TryInit(bufferWriter, header, fBits, defs);
        Assert.True(initResult, "Initialization failed");
        
        var appendResult = writer.TryAppend(bufferWriter, loggedDataToken);
        Assert.True(appendResult, "Appending ULogLoggedDataMessageToken failed");
        
        Assert.False(writer.TryAppend(bufferWriter, definitionFailedToken));
        
        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);
        
        var tokenCount = 0;
        while (reader.TryRead(ref rdr, out var token))
        {
            tokenCount++;
            _output.WriteLine($"Read token: {token?.TokenName}");
        }

        Assert.Equal(5, tokenCount);
    }
}