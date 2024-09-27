using System.Buffers;
using Asv.ULog;
using Xunit.Abstractions;

namespace Asv.Ulog.Tests;

public class ULogReaderTests
{
    private readonly ITestOutputHelper _output;

    public ULogReaderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Calculate_ulog_file_statistic()
    {
        var bytes = TestData.ulog_sample;
        var data = new ReadOnlySequence<byte>(bytes);
        var rdr = new SequenceReader<byte>(data); 
        var reader = ULog.ULog.CreateReader();
        int index = 0;
        var stat = Enum.GetValues<ULogToken>().ToDictionary(token => token, token => 0);
        while (reader.TryRead(ref rdr, out var token))
        {
            Assert.NotNull(token);
            stat[token.TokenType] += 1;
            index++;
        } 
        _output.WriteLine($"Read {index} tokens");
        foreach (var (key, value) in stat)
        {
            _output.WriteLine($"{key} : {value}");
        }
    }
    
    [Fact]
    public void TryRead_HasCorruptedBytes_Success() // TODO: find out why reader doesn't read all tokens
    {
        var bytes = TestData.ulog_sample;
        var wrongBytesCount = 199;
        var random = new Random(4);
        var wrongBytes = new byte[wrongBytesCount];
        random.NextBytes(wrongBytes);
        var part1 = bytes[..(bytes.Length / 2)];
        var part2 = bytes[(bytes.Length / 2)..];

        var bytesBig = part1
            .Concat(wrongBytes)
            .Concat(ULogSynchronizationMessageToken.FullMessage)
            .Concat(part2)
            .ToArray();
        
        var data = new ReadOnlySequence<byte>(bytesBig);
        var rdr = new SequenceReader<byte>(data); 
        var reader = ULog.ULog.CreateReader();
        int index = 0;
        var stat = Enum.GetValues<ULogToken>().ToDictionary(token => token, token => 0);
        while (reader.TryRead(ref rdr, out var token))
        {
            if (token is null)
            {
                continue;
            }
            
            stat[token.TokenType] += 1;
            index++;
        } 
        _output.WriteLine($"Read {index} tokens");
        foreach (var (key, value) in stat)
        {
            _output.WriteLine($"{key} : {value}");
        }
    }
   
    [Fact]
    public void Read_ulog_file_header_and_flag_token_with_check_values()
    {
        var data = new ReadOnlySequence<byte>(TestData.ulog_sample);
        var rdr = new SequenceReader<byte>(data);
        var reader = ULog.ULog.CreateReader();

        var result = reader.TryRead<ULogFileHeaderToken>(ref rdr, out var header);
        Assert.True(result);
        Assert.NotNull(header);
        Assert.Equal(ULogToken.FileHeader,header.TokenType);
        Assert.Equal(20309082U, header.Timestamp);
        Assert.Equal(1,header.Version);

        result = reader.TryRead<ULogFlagBitsMessageToken>(ref rdr, out var flag);
        Assert.True(result);
        Assert.NotNull(flag);  
        Assert.Equal(ULogToken.FlagBits,flag.TokenType);
    }

}