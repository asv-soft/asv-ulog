using System.Buffers;
using System.Globalization;
using System.Text;
using Asv.ULog.Information;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Asv.ULog.Shell;

public class UlogFileReader
{
    public void ReadULogFile(string filePath)
    {
        var rdr = CreateSequenceReader(filePath);
        var reader = ULog.CreateReader();
        var index = 0;
        var stat = Enum.GetValues<ULogToken>().ToDictionary(token => token, token => 0);
        while (reader.TryRead(ref rdr, out var token))
        {
            stat[token.TokenType] += 1;
            index++;
        } 
        AnsiConsole.MarkupLine($"[green]Total tokens read:[/] [bold red]{index}[/]");
        var table = new Table();
        table.AddColumns("[blue]Tokens[/]", "[green]Number of tokens[/]");
        table.Border(TableBorder.Double);
        foreach (var (key, value) in stat)
        {
            table.AddRow(new Markup($"[blue]{key}[/]"), new Markup($"[green]{value}[/]"));
        }
        AnsiConsole.Write(table);
    }



    public void ReadParams(string filePath)
    {
        var rdr = CreateSequenceReader(filePath);
        var reader = ULog.CreateReader();
        var index = 0;
        var paramsDict = new Dictionary<string, IList<(ULogType,byte[])>>();
        while (reader.TryRead(ref rdr, out var token))
        {
            if (token.TokenType == ULogToken.Parameter)
            {
                if (token is ULogParameterMessageToken param)
                {
                    index++;
                    if (!paramsDict.ContainsKey(param.Key.Name))
                    {
                        paramsDict[param.Key.Name] = new List<(ULogType,byte[])> { new (param.Key.Type.BaseType,param.Value) }; 
                        continue;
                    }

                    paramsDict[param.Key.Name].Add(new (param.Key.Type.BaseType,param.Value));
                }
            }
        }
        AnsiConsole.Markup($"[green]Total params read:[/] [bold red]{index}[/]\n");

        var table = new Table();
        table.AddColumns("[blue]Parameter[/]", "[green]Value[/]");
        table.Border(TableBorder.Double);
        foreach (var param in paramsDict)
        {
            var sb = new StringBuilder();
            foreach (var v in param.Value)
            {
                sb.Append(ULog.GetSimpleValue(v.Item1,v.Item2));
            }
            table.AddRow(new Markup($"[blue]{param.Key}[/]"), new Markup($"[red]{sb}[/]"));
        }
        AnsiConsole.Write(table);
    }
    
    public void ReadInfoMessages(string filePath)
    {
        var rdr = CreateSequenceReader(filePath);
        var reader = ULog.CreateReader();
        var index = 0;
        var paramsDict = new Dictionary<string, IList<(ULogType,byte[])>>();
        while (reader.TryRead(ref rdr, out var token))
        {
            if (token != null && token.TokenType != ULogToken.Information) continue;
            if (token is not ULogInformationMessageToken param) continue;
            if (!paramsDict.TryGetValue(param.Key.Name, out IList<(ULogType, byte[])>? value))
            {
                value = new List<(ULogType,byte[])>
                {
                    new (param.Key.Type.BaseType,param.Value)
                };
                paramsDict[param.Key.Name] = value;
                index++;
                continue;
            }

            value.Add(new ValueTuple<ULogType, byte[]>(param.Key.Type.BaseType, param.Value));
        }
        AnsiConsole.MarkupLine($"[green]Total info messages read:[/] [bold red]{index}[/]");
        AnsiConsole.MarkupLine("[blue]Info messages:[/]");
        
        foreach (var param in paramsDict)
        {
            var sb = new StringBuilder();
            foreach (var v in param.Value)
            {
                if (v.Item1 == ULogType.Char)
                {
                    var type = CharToString(v.Item2).ToString();
                    sb.Append(type);
                }
                else
                {
                    sb.Append(ULog.GetSimpleValue(v.Item1,v.Item2));
                }
            }
            AnsiConsole.MarkupLine($"[blue]{param.Key}[/]: [red]{sb.ToString().Replace("[", "[[").Replace("]", "]]")}[/]");

        }
    }
    public void ReadMessages(string filePath)
    {
        var rdr = CreateSequenceReader(filePath);
        var reader = ULog.CreateReader();
        var index = 0;
        while (reader.TryRead(ref rdr, out var token))
        {
            if (token.TokenType == ULogToken.LoggedString)
            {
                index++;
                if (token is ULogLoggedStringMessageToken msg)
                {
                    var time = new TimeSpan((long)msg.TimeStamp * 10);
                    AnsiConsole.MarkupLine($"[blue]{time:hh\\:mm\\:ss}[/]: [yellow]{msg.LogLevel}[/] [green]{msg.Message.Replace("[", "[[").Replace("]","]]")}[/]");
                }
            }
        }
    }
    
    private SequenceReader<byte> CreateSequenceReader(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var data = new ReadOnlySequence<byte>(fileBytes);
        return new SequenceReader<byte>(data); 
    }
    
    private ReadOnlySpan<char> CharToString(byte[] value)
    {
        var charSize = ULog.Encoding.GetCharCount(value);
        var charBuffer = new char[charSize];
        ULog.Encoding.GetChars(value,charBuffer);
        var rawString = new ReadOnlySpan<char>(charBuffer, 0, charSize);
        return rawString.ToString();
    }
}