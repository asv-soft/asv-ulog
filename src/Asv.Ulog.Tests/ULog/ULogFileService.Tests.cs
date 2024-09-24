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
        _output.WriteLine($"Is Corrupted {uLogFileService}");
        _output.WriteLine("\n===============Definition===============");
        //_output.WriteLine($"{result.FlagBits.TokenName}");
        
        foreach (var definition in uLogFileService.Definition.Format)
        {
            foreach (var fields in definition)
            {
                _output.WriteLine($"{fields.Key} = {fields.Value}");
            }
        }

        _output.WriteLine("\n===============Definition and Data===============");
        foreach (var defData in uLogFileService.DefinitionAndData)
        {
            switch (defData.TokenType)
            {
                case ULogToken.Information:
                    _output.WriteLine(GetInformation((ULogInformationMessageToken)defData));
                    break;
                case ULogToken.Parameter:
                    _output.WriteLine(GetParameter((ULogParameterMessageToken)defData));
                    break;
            }
        }

        foreach (var param in uLogFileService.Parameters)
        {
            _output.WriteLine($"{uLogFileService.Time} {param.TokenName} {param.Value}");
        }
    }

    private ValueType ParameterTokenValueToValueType(ULogType typeBaseType, byte[] value)
    {
        switch (typeBaseType)
        {
            case ULogType.Float:
                var single = BitConverter.ToSingle(value);
                return single;
            case ULogType.Int32:
                var int32 = BitConverter.ToInt32(value);
                return int32;
            default:
                throw new ArgumentException("Wrong ulog value type for ParameterTokenValue");
        }
    }

    public string GetParameter(ULogParameterMessageToken param)
    {
        var result = new StringBuilder();
        result.Append("Parameter : ");
        var value = ValueToString(param.Key.Type.BaseType, param.Value);
        return result.Append($"{param.Key.Name} = {value}").ToString();
    }

    public string GetFormat(ULogFormatMessageToken format)
    {
        var result = new StringBuilder();
        result.Append($"{format.TokenName} {format.Fields.Count} items\n [\n");
        foreach (var field in format.Fields)
        {
            result.Append($"{field.Name} = {field.Type.TypeName}\n");
        }

        result.Append("]");
        return result.ToString();
    }

    public string GetInformation(ULogInformationMessageToken information)
    {
        var result = new StringBuilder();
        result.Append($"{information.TokenName} : ");
        var str = ValueToString(information.Key.Type.BaseType, information.Value);
        result.Append($"{information.Key.Name} = {str}");

        return result.ToString();
    }

    private string ValueToString(ULogType type, byte[] value)
    {
        switch (type)
        {
            case ULogType.UInt32:
            case ULogType.Int32:
                return BitConverter.ToInt32(value).ToString(CultureInfo.InvariantCulture);
            case ULogType.Char:
                return CharToString(value).ToString();
            case ULogType.Float:
                return BitConverter.ToSingle(value).ToString();
            default:
                throw new ArgumentNullException("Wrong ulog value type for InformationTokenValue");
        }
    }

    private ReadOnlySpan<char> CharToString(byte[] value)
    {
        var charSize = ULog.ULog.Encoding.GetCharCount(value);
        var charBuffer = new char[charSize];
        ULog.ULog.Encoding.GetChars(value, charBuffer);
        var rawString = new ReadOnlySpan<char>(charBuffer, 0, charSize);
        return rawString.ToString();
    }
}