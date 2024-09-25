using System.Buffers;
using System.Globalization;
using System.Text;
using Asv.ULog;
using Asv.ULog.Information;
using Xunit.Abstractions;

namespace Asv.Ulog.Tests;

public class ULogFileService_Tests
{
    private readonly ITestOutputHelper _output;

    public ULogFileService_Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void File_service_read_all()
    {
        var reader = ULog.ULog.CreateReader();
        var data = new ReadOnlySequence<byte>(TestData.ulog_log_small);
        var uLogFileService = new ULogFileService();
        uLogFileService.Load(ref data, reader);

        _output.WriteLine("===============Header===============");
        _output.WriteLine($"Version: {uLogFileService.Version}");
        _output.WriteLine($"Time: {uLogFileService.Time}");
        _output.WriteLine($"Unknown tokens found: {uLogFileService.Unknown.Count}");
        _output.WriteLine("\n===============Definition===============");
        foreach (var definition in uLogFileService.Definition.Format)
        {
            _output.WriteLine($"Format:");
            foreach (var fields in definition)
            {
                _output.WriteLine($"{fields.Key} = {fields.Value}");
            }
        }
        foreach (var info in uLogFileService.Definition.Information)
        {
            
            _output.WriteLine($"Information: {info.Key}: {info.Value}");
        }

        foreach (var multiInformation in uLogFileService.Definition.MultiInformation)
        {
            _output.WriteLine($"MultiInformation: {multiInformation.Key} ' ");
            foreach (var s in multiInformation.Value)
            {
                _output.WriteLine($"{s}");
            }
            _output.WriteLine("'");
        }
        _output.WriteLine("\n===============Definition and Data===============");

        foreach (var param in uLogFileService.Definition.Parameters)
        {
            _output.WriteLine($"param '{param.Key}' = {param.Value}");
        }
    }
}