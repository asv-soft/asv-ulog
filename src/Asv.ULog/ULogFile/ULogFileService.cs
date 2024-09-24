using System.Buffers;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Asv.ULog.Information;
using ZLogger;

namespace Asv.ULog;

public class ULogFileService : ULogFileModel
{
    private readonly IULogReader _reader;
    private ReadOnlySequence<byte> _data;
    public List<ULogParameterMessageToken> Parameters { get; set; } = new();

    public ULogFileService()
    {
    }

    private Dictionary<string, ULogType> GetFormatDictionary(ULogFormatMessageToken format)
    {
        var dictionary = new Dictionary<string, ULogType>();

        foreach (var field in format.Fields)
        {
            if (field.Name.Contains("_padding")) continue;
            dictionary.Add(field.Name, field.Type.BaseType);
        }

        foreach (var item in dictionary)
        {
            if (item.Value is ULogType.ReferenceType)
            {
                var reference = dictionary.FirstOrDefault(_ => _.Key.Equals(item.Value.ToString()));
                //item = new KeyValuePair<string,ULogType>(item.Key,reference.Value);
            }
        }

        return dictionary;
    }

    private KeyValuePair<string, string> GetInformationDictionary(ULogInformationMessageToken information)
    {
        return new KeyValuePair<string, string>(information.Key.Name,
            ValueToString(information.Key.Type.BaseType, information.Value));
    }

    private KeyValuePair<string, ICollection<string>> GetMultiInformationDictionary(
        ULogMultiInformationMessageToken information)
    {
        var collection = new List<string>();
        collection.Add(ValueToString(information.Key.Type.BaseType, information.Value));
        if (information.IsContinued != 1)
        {
            return new KeyValuePair<string, ICollection<string>>(information.Key.Name, collection);
        }
        var references = Definition.MultiInformation.First(_ => _.Key.Equals(information.Key.Name));
        Definition.MultiInformation.Remove(references);
            foreach (var refer in references.Value)
            {
                collection.Add($"{refer}");
            }
        return new KeyValuePair<string, ICollection<string>>(information.Key.Name, collection);
    }

    public string GetParameter(ULogParameterMessageToken param)
    {
        var result = new StringBuilder();
        result.Append("Parameter : ");
        var value = ValueToString(param.Key.Type.BaseType, param.Value);
        return result.Append($"{param.Key.Name} = {value}").ToString();
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
            default:
                throw new ArgumentNullException("Wrong ulog value type for InformationTokenValue");
        }
    }

    private ReadOnlySpan<char> CharToString(byte[] value)
    {
        var charSize = ULog.Encoding.GetCharCount(value);
        var charBuffer = new char[charSize];
        ULog.Encoding.GetChars(value, charBuffer);
        var rawString = new ReadOnlySpan<char>(charBuffer, 0, charSize);
        return rawString.ToString();
    }

    public void Load(ref ReadOnlySequence<byte> data, IULogReader reader)
    {
        Definition = new();
        Definition.Format = new Collection<IDictionary<string, ULogType>>();
        Definition.Information = new Dictionary<string, string>();
        Definition.MultiInformation = new Dictionary<string, ICollection<string>>();
        Data = new List<IULogToken>();
        DefinitionAndData = new List<IULogToken>();
        Unknown = new List<IULogToken>();
        var rdr = new SequenceReader<byte>(data);
        while (reader.TryRead(ref rdr, out var token))
        {
            switch (token.TokenSection)
            {
                case TokenPlaceFlags.Header:
                    var header = token as ULogFileHeaderToken;
                    Version = header.Version;
                    Time = FromTimeStampToDateTime(header.Timestamp).TimeOfDay;
                    break;
                case TokenPlaceFlags.Definition:
                    switch (token.TokenType)
                    {
                        case ULogToken.FlagBits:
                            break;
                        case ULogToken.Format:
                            Definition.Format.Add(GetFormatDictionary((ULogFormatMessageToken)token));
                            break;
                        case ULogToken.Information:
                            Definition.Information.Add(GetInformationDictionary((ULogInformationMessageToken)token));
                            break;
                        case ULogToken.MultiInformation:
                            Definition.MultiInformation.Add(
                                GetMultiInformationDictionary((ULogMultiInformationMessageToken)token));
                            break;
                    }
                    break;
                case (TokenPlaceFlags.DefinitionAndData):
                    switch (token.TokenType)
                    {
                        case ULogToken.Information:
                            Definition.Information.Add(GetInformationDictionary((ULogInformationMessageToken)token));
                            break;
                        case ULogToken.MultiInformation:
                            Definition.MultiInformation.Add(
                                GetMultiInformationDictionary((ULogMultiInformationMessageToken)token));
                            break;
                    }

                    break;
            }
        }
    }

    public ULogFileModel Save()
    {
        throw new NotImplementedException();
    }

    private DateTime FromTimeStampToDateTime(ulong timestamp)
    {
        var dt = new DateTime();
        dt = dt.AddSeconds(timestamp).ToLocalTime();
        return dt;
    }
}

public class ULogFileModel : IULogFile
{
    public TimeSpan Time { get; set; }
    public byte Version { get; set; }
    public Definition Definition { get; set; }

    public ICollection<IULogToken> Subscription { get; set; }
    public ICollection<IULogToken> Data { get; set; }
    public ICollection<IULogToken> DefinitionAndData { get; set; }
    public ICollection<IULogToken> Unknown { get; set; }
}

public class Definition
{
    public ICollection<IDictionary<string, ULogType>> Format { get; set; }
    public IDictionary<string, string> Information { get; set; }
    public IDictionary<string, ICollection<string>> MultiInformation { get; set; }
}

public interface IULogFile
{
    public TimeSpan Time { get; set; }
    public byte Version { get; set; }
    public Definition Definition { get; set; }
    public ICollection<IULogToken> DefinitionAndData { get; set; }
    public ICollection<IULogToken> Subscription { get; set; }
    public ICollection<IULogToken> Data { get; set; }
}