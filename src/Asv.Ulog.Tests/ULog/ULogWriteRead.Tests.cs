using System.Buffers;
using System.Collections.Immutable;
using System.Security.Cryptography;
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
                        TypeName = "int32"
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

        writer.AddHeaderAndFlagBits(bufferWriter, header, fBits);
        writer.AppendDefinition(bufferWriter, format1);
        writer.AppendDefinition(bufferWriter, format2);
        writer.AppendDefinition(bufferWriter, infoToken);
        writer.AppendDefinition(bufferWriter, paramToken);
        writer.AppendData(bufferWriter, loggedDataToken);
        writer.AppendData(bufferWriter, loggedStringToken);
        writer.AppendData(bufferWriter, syncToken);
        writer.AppendData(bufferWriter, dropoutToken);
        
        var hash1 = ComputeHash(bufferWriter.WrittenSpan.ToArray());

        var data = new ReadOnlySequence<byte>(bufferWriter.WrittenSpan.ToArray());
        var rdr = new SequenceReader<byte>(data);


        var tokenCount = 0;
        while (reader.TryRead(ref rdr, out var token))
        {
            tokenCount++;
            _output.WriteLine($"Read token: {token?.TokenName}");
        }

        Assert.Equal(10, tokenCount); 
        
        var bufferWriter2 = new ArrayBufferWriter<byte>();

        writer.AddHeaderAndFlagBits(bufferWriter2, header, fBits);
        writer.AppendDefinition(bufferWriter2, format1);
        writer.AppendDefinition(bufferWriter2, format2);
        writer.AppendDefinition(bufferWriter2, infoToken);
        writer.AppendDefinition(bufferWriter2, paramToken);
        writer.AppendData(bufferWriter2, loggedDataToken);
        writer.AppendData(bufferWriter2, loggedStringToken);
        writer.AppendData(bufferWriter2, syncToken);
        writer.AppendData(bufferWriter2, dropoutToken);

        var hash2 = ComputeHash(bufferWriter2.WrittenSpan.ToArray());
        
        Assert.Equal(hash1, hash2);
        
    }
    
    private static string ComputeHash(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}