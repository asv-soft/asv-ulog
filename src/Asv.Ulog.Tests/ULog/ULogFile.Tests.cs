using System.Buffers;
using Asv.ULog.ULogFile;
using Xunit.Abstractions;

namespace Asv.Ulog.Tests;

public class ULogFileTests
{
    private readonly ITestOutputHelper _output;

    public ULogFileTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Deserialize_AllData_Success()
    {
        var reader = ULog.ULog.CreateReader();
        var data = new ReadOnlySequence<byte>(TestData.ulog_log_small);
        var uLogFile = new ULogFile(reader);
        uLogFile.Deserialize(ref data);

        _output.WriteLine("===============Header===============");
        _output.WriteLine($"Version: {uLogFile.Version}");
        _output.WriteLine($"Time: {uLogFile.Time}");
        _output.WriteLine("===============Definition===============");
        _output.WriteLine("===============Flag Bits===============");
        _output.WriteLine("CompatFlags:");
        foreach (var b in uLogFile.Definition.FlagBits.CompatFlags)
        {
            _output.WriteLine($"{b}");
        }
        _output.WriteLine("Incompat flags:");
        foreach (var b in uLogFile.Definition.FlagBits.IncompatFlags)
        {
            _output.WriteLine($"{b}");
        }
        _output.WriteLine("Appended offsets:");
        foreach (var b in uLogFile.Definition.FlagBits.AppendedOffsets)
        {
            _output.WriteLine($"{b}");
        }
    }
}