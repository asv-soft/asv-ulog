using System.Buffers;
using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Asv.ULog;
using Asv.ULog.Information;
using Asv.Ulog.Tests;
using Xunit.Abstractions;

namespace Asv.ULog.Tests;

public class ULogWriteReadTests
{
    private readonly ITestOutputHelper _output;
    private readonly IULogReader _reader;
    private readonly IULogWriter _writer;

    public ULogWriteReadTests(ITestOutputHelper output)
    {
        _output = output;
        _reader = ULog.CreateReader();
        _writer = ULog.CreateWriter();
    }

    [Fact]
    public void WriteReadFullULog_Success()
    {

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


        var format1 = new ULogFormatMessageToken
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
        };
        var format2 = new ULogFormatMessageToken
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
                        TypeName = "int32_t"
                    },
                }
            },
            MessageName = "Test2"
        };

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
            Value = [1, 2, 3, 4, 5, 6, 7, 8]
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

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, format1);
        _writer.AppendDefinition(bufferWriter, format2);
        _writer.AppendDefinition(bufferWriter, infoToken);
        _writer.AppendDefinition(bufferWriter, paramToken);
        _writer.AppendData(bufferWriter, loggedDataToken);
        _writer.AppendData(bufferWriter, loggedStringToken);
        _writer.AppendData(bufferWriter, syncToken);
        _writer.AppendData(bufferWriter, dropoutToken);
        
        var written = new List<IULogToken>(
            [header, fBits, format1, format2, infoToken, 
                paramToken, loggedDataToken, loggedStringToken, syncToken, dropoutToken
            ]);
        
        var hash1 = ComputeHash(bufferWriter.WrittenSpan.ToArray());

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        var read = new List<IULogToken>();
        var tokenCount = 0;
        while (_reader.TryRead(ref rdr, out var token))
        {
            tokenCount++;
            read.Add(token);
            _output.WriteLine($"Read token: {token?.TokenName}");
        }

        Assert.Equal(10, tokenCount); 
        
        var bufferWriter2 = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter2, header, fBits);
        _writer.AppendDefinition(bufferWriter2, format1);
        _writer.AppendDefinition(bufferWriter2, format2);
        _writer.AppendDefinition(bufferWriter2, infoToken);
        _writer.AppendDefinition(bufferWriter2, paramToken);
        _writer.AppendData(bufferWriter2, loggedDataToken);
        _writer.AppendData(bufferWriter2, loggedStringToken);
        _writer.AppendData(bufferWriter2, syncToken);
        _writer.AppendData(bufferWriter2, dropoutToken);

        var hash2 = ComputeHash(bufferWriter2.WrittenSpan.ToArray());
        
        Assert.Equal(hash1, hash2);

        for (var i = 0; i < read.Count; i++) Assert.True(read[i].Equals(written[i]));
    }

    [Fact]
    public void RewriteFullTestFile()
    {
        var bytes = TestData.ulog_with_px4_events;
        var data = new ReadOnlySequence<byte>(bytes);
        var rdr = new SequenceReader<byte>(data); 
        var reader = ULog.CreateReader();
        var tokens = new List<IULogToken>();
        while (reader.TryRead(ref rdr, out var token))
        {
            Assert.NotNull(token);
            tokens.Add(token);
        }
        
        var buffer = new ArrayBufferWriter<byte>();

        ULogFileHeaderToken? header = null;
        ULogFlagBitsMessageToken? fBits = null;
        var isHeaderWrote = false;
        foreach (var token in tokens)
        {
            if (header != null && fBits != null && !isHeaderWrote)
            {
                _writer.AddHeaderAndFlagBits(buffer, header, fBits);
                isHeaderWrote = true;
            }
            
            if (token.TokenType == ULogToken.FileHeader)
            {
                header = token as ULogFileHeaderToken;
                continue;
            }

            if (token.TokenType == ULogToken.FlagBits)
            {
                fBits = token as ULogFlagBitsMessageToken;
                continue;
            }

            if (token.TokenSection.HasFlag(TokenPlaceFlags.Definition))
            {
                _writer.AppendDefinition(buffer, token);
                continue;
            }

            if (token.TokenSection.HasFlag(TokenPlaceFlags.Data))
            {
                _writer.AppendData(buffer, token);
                continue;
            }
        }
        File.WriteAllBytes("ulog_write_read_test.ulg", buffer.WrittenMemory.ToArray());
        
        Assert.Equal(ComputeHash(buffer.WrittenMemory.ToArray()), ComputeHash(bytes));
    }
    
    private static string ComputeHash(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    
    [Fact]
    public void ULogFileHeaderToken_Equals_Success()
    {
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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        var read = new List<IULogToken>();
        while (_reader.TryRead(ref rdr, out var token))
        {
            Assert.NotNull(token);
            read.Add(token);
            _output.WriteLine($"Read token: {token.TokenName}");
        }
        
        Assert.Equal(read[0], header);
        Assert.Equal(read[1], fBits);
    }
    
    [Fact]
    public void ULogFileHeaderToken_Equals_Failure()
    {
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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        var read = new List<IULogToken>();
        while (_reader.TryRead(ref rdr, out var token))
        {
            Assert.NotNull(token);
            read.Add(token);
            _output.WriteLine($"Read token: {token.TokenName}");
        }
        
        var fakeHeader = new ULogFileHeaderToken
        {
            Timestamp = 22222UL,
            Version = 2
        };
        
        var fakeFBits = new ULogFlagBitsMessageToken
        {
            CompatFlags = [4, 42, 5, 4, 5, 6, 0, 0],
            IncompatFlags = [1, 2, 3, 2, 5, 6, 7, 8],
            AppendedOffsets = [0UL, 3UL, 3UL]
        };
        
        Assert.NotEqual(read[0], fakeHeader);
        Assert.NotEqual(read[1], fakeFBits);
    }

    [Fact]
    public void ULogFormatMessageToken_Equals_Success()
    {
        var token = new ULogFormatMessageToken
        {
            MessageName = "TestMessage",
            Fields =
            {
                new ULogTypeAndNameDefinition
                {
                    Name = "TestField",
                    Type = new ULogTypeDefinition
                    {
                        TypeName = "float",
                        BaseType = ULogType.Float
                    }
                }
            }
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.Format) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogFormatMessageToken_Equals_Failure()
    {
        var token = new ULogFormatMessageToken
        {
            MessageName = "TestMessage",
            Fields =
            {
                new ULogTypeAndNameDefinition
                {
                    Name = "TestField",
                    Type = new ULogTypeDefinition
                    {
                        TypeName = "float",
                        BaseType = ULogType.Float
                    }
                }
            }
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogFormatMessageToken
            {
                MessageName = "FakeMessage",
                Fields =
                {
                    new ULogTypeAndNameDefinition
                    {
                        Name = "FakeField",
                        Type = new ULogTypeDefinition
                        {
                            TypeName = "int",
                            BaseType = ULogType.Int32
                        }
                    }
                }
            };
            if (readToken?.TokenType == ULogToken.Format) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogParameterMessageToken_Equals_Success()
    {
        var token = new ULogParameterMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "Param1",
                Type = new ULogTypeDefinition
                {
                    TypeName = "float",
                    BaseType = ULogType.Float
                }
            },
            Value = BitConverter.GetBytes(12.34f)
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.Parameter) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogParameterMessageToken_Equals_Failure()
    {
        var token = new ULogParameterMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "Param1",
                Type = new ULogTypeDefinition
                {
                    TypeName = "float",
                    BaseType = ULogType.Float
                }
            },
            Value = BitConverter.GetBytes(12.34f)
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogParameterMessageToken
            {
                Key = new ULogTypeAndNameDefinition
                {
                    Name = "ParamFake",
                    Type = new ULogTypeDefinition
                    {
                        TypeName = "int",
                        BaseType = ULogType.Int32
                    }
                },
                Value = BitConverter.GetBytes(9999)
            };
            if (readToken?.TokenType == ULogToken.Parameter) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogInformationMessageToken_Equals_Success()
    {
        var token = new ULogInformationMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "InfoKey",
                Type = new ULogTypeDefinition
                {
                    TypeName = "char",
                    BaseType = ULogType.Char
                }
            },
            Value = [1, 2, 3, 4, 5, 6, 7, 8]
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.Information) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogInformationMessageToken_Equals_Failure()
    {
        var token = new ULogInformationMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "InfoKey",
                Type = new ULogTypeDefinition
                {
                    TypeName = "char",
                    BaseType = ULogType.Char
                }
            },
            Value = [1, 2, 3, 4, 5, 6, 7, 8]
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogInformationMessageToken
            {
                Key = new ULogTypeAndNameDefinition
                {
                    Name = "FakeInfo",
                    Type = new ULogTypeDefinition
                    {
                        TypeName = "int",
                        BaseType = ULogType.Int32
                    }
                },
                Value = [99, 98, 97, 96],
            };
            if (readToken?.TokenType == ULogToken.Information) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogMultiInformationMessageToken_Equals_Success()
    {
        var token = new ULogMultiInformationMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "MultiInfo",
                Type = new ULogTypeDefinition
                {
                    TypeName = "char",
                    BaseType = ULogType.Char
                }
            },
            Value = [1, 2, 3, 4],
            IsContinued = 1
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.MultiInformation) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogMultiInformationMessageToken_Equals_Failure()
    {
        var token = new ULogMultiInformationMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "MultiInfo",
                Type = new ULogTypeDefinition
                {
                    TypeName = "char[4]",
                    BaseType = ULogType.Char
                }
            },
            Value = [1, 2, 3, 4],
            IsContinued = 1
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogMultiInformationMessageToken
            {
                Key = new ULogTypeAndNameDefinition
                {
                    Name = "FakeMultiInfo",
                    Type = new ULogTypeDefinition
                    {
                        TypeName = "int[4]",
                        BaseType = ULogType.Int32
                    }
                },
                Value = [9, 8, 7, 6],
                IsContinued = 0
            };
            if (readToken?.TokenType == ULogToken.MultiInformation) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogDefaultParameterMessageToken_Equals_Success()
    {
        var token = new ULogDefaultParameterMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "DefaultParam",
                Type = new ULogTypeDefinition
                {
                    TypeName = "float",
                    BaseType = ULogType.Float
                }
            },
            Value = BitConverter.GetBytes(123.45f),
            DefaultType = ULogParameterDefaultTypes.SystemWide
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.DefaultParameter) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogDefaultParameterMessageToken_Equals_Failure()
    {
        var token = new ULogDefaultParameterMessageToken
        {
            Key = new ULogTypeAndNameDefinition
            {
                Name = "DefaultParam",
                Type = new ULogTypeDefinition
                {
                    TypeName = "float",
                    BaseType = ULogType.Float
                }
            },
            Value = BitConverter.GetBytes(123.45f),
            DefaultType = ULogParameterDefaultTypes.SystemWide
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogDefaultParameterMessageToken
            {
                Key = new ULogTypeAndNameDefinition
                {
                    Name = "FakeDefaultParam",
                    Type = new ULogTypeDefinition
                    {
                        TypeName = "int",
                        BaseType = ULogType.Int32
                    }
                },
                Value = BitConverter.GetBytes(555),
                DefaultType = ULogParameterDefaultTypes.ForCurrentConfiguration
            };
            if (readToken?.TokenType == ULogToken.DefaultParameter) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogSubscriptionMessageToken_Equals_Success()
    {
        var token = new ULogSubscriptionMessageToken
        {
            MessageName = "SensorData",
            MessageId = 1,
            MultiId = 0
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.Subscription) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogSubscriptionMessageToken_Equals_Failure()
    {
        var token = new ULogSubscriptionMessageToken
        {
            MessageName = "SensorData",
            MessageId = 1,
            MultiId = 0
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogSubscriptionMessageToken
            {
                MessageName = "FakeSensorData",
                MessageId = 99,
                MultiId = 1
            };
            if (readToken?.TokenType == ULogToken.Subscription) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogUnsubscriptionMessageToken_Equals_Success()
    {
        var token = new ULogUnsubscriptionMessageToken
        {
            MessageId = 100
        };

        var bufferWriter = new ArrayBufferWriter<byte>();
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogUnsubscriptionMessageToken_Equals_Failure()
    {
        var token = new ULogUnsubscriptionMessageToken
        {
            MessageId = 100
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogUnsubscriptionMessageToken
            {
                MessageId = 999
            };
            if (readToken?.TokenType == ULogToken.Unsubscription) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogLoggedDataMessageToken_Equals_Success()
    {
        var token = new ULogLoggedDataMessageToken
        {
            MessageId = 1,
            Data = [1, 2, 3, 4]
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.LoggedData) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogLoggedDataMessageToken_Equals_Failure()
    {
        var token = new ULogLoggedDataMessageToken
        {
            MessageId = 1,
            Data = [1, 2, 3, 4]
        };

        var bufferWriter = new ArrayBufferWriter<byte>();
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogLoggedDataMessageToken
            {
                MessageId = 99,
                Data = [9, 8, 7, 6]
            };
            Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogLoggedStringMessageToken_Equals_Success()
    {
        var token = new ULogLoggedStringMessageToken
        {
            LogLevel = ULogLoggedStringMessageToken.ULogLevel.Debug,
            TimeStamp = 123456789UL,
            Message = "Debugging output"
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.LoggedString) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogLoggedStringMessageToken_Equals_Failure()
    {
        var token = new ULogLoggedStringMessageToken
        {
            LogLevel = ULogLoggedStringMessageToken.ULogLevel.Debug,
            TimeStamp = 123456789UL,
            Message = "Debugging output"
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogLoggedStringMessageToken
            {
                LogLevel = ULogLoggedStringMessageToken.ULogLevel.Err,
                TimeStamp = 987654321UL,
                Message = "Error occurred"
            };
            if (readToken?.TokenType == ULogToken.LoggedString) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogTaggedLoggedStringMessageToken_Equals_Success()
    {
        var token = new ULogTaggedLoggedStringMessageToken
        {
            LogLevel = ULogTaggedLoggedStringMessageToken.ULogLevel.Info,
            Tag = 1,
            Timestamp = 12345678UL,
            Message = "Info message"
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            if (readToken?.TokenType == ULogToken.TaggedLoggedString) Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogTaggedLoggedStringMessageToken_Equals_Failure()
    {
        var token = new ULogTaggedLoggedStringMessageToken
        {
            LogLevel = ULogTaggedLoggedStringMessageToken.ULogLevel.Info,
            Tag = 1,
            Timestamp = 12345678UL,
            Message = "Info message"
        };

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
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        _writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogTaggedLoggedStringMessageToken
            {
                LogLevel = ULogTaggedLoggedStringMessageToken.ULogLevel.Alert,
                Tag = 99,
                Timestamp = 87654321UL,
                Message = "Alert message"
            };
            if (readToken?.TokenType == ULogToken.TaggedLoggedString) Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogDropoutMessageToken_Equals_Success()
    {
        var token = new ULogDropoutMessageToken
        {
            Duration = 100
        };

        var bufferWriter = new ArrayBufferWriter<byte>();
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            Assert.Equal(token, readToken);
        }
    }

    [Fact]
    public void ULogDropoutMessageToken_Equals_Failure()
    {
        var token = new ULogDropoutMessageToken
        {
            Duration = 100
        };

        var bufferWriter = new ArrayBufferWriter<byte>();
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            var fakeToken = new ULogDropoutMessageToken
            {
                Duration = 999
            };
            Assert.NotEqual(fakeToken, readToken);
        }
    }
    
    [Fact]
    public void ULogSynchronizationMessageToken_Equals_Success()
    {
        var token = new ULogSynchronizationMessageToken();

        var bufferWriter = new ArrayBufferWriter<byte>();
        _writer.AppendDefinition(bufferWriter, token);

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);

        while (_reader.TryRead(ref rdr, out var readToken))
        {
            Assert.Equal(token, readToken);
        }
    }
}