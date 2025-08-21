using System.Buffers;
using System.Text;
using Asv.ULog.Information;
using Spectre.Console;

namespace Asv.ULog.Shell;

public class UlogFileReader
{
    public void ReadULogFile(string filePath)
    {
        var tokens = ReadTokens(filePath, token => true);
        var index = tokens.Count;
        var stat = tokens.GroupBy(t => t.TokenType)
            .ToDictionary(g => g.Key, g => g.Count());

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
        var tokens = ReadTokens(filePath, token => token.TokenType == ULogToken.Parameter && token is ULogParameterMessageToken);
        var paramsDict = new Dictionary<string, IList<(ULogType, byte[])>>();
        foreach (var token in tokens.OfType<ULogParameterMessageToken>())
        {
            if (!paramsDict.ContainsKey(token.Key.Name))
            {
                paramsDict[token.Key.Name] = new List<(ULogType, byte[])> { (token.Key.Type.BaseType, token.Value) };
            }
            else
            {
                paramsDict[token.Key.Name].Add((token.Key.Type.BaseType, token.Value));
            }
        }
        
        AnsiConsole.Markup($"[green]Total params read:[/] [bold red]{tokens.Count}[/]\n");
        var table = new Table();
        table.AddColumns("[blue]Parameter[/]", "[green]Value[/]");
        table.Border(TableBorder.Double);
        foreach (var param in paramsDict)
        {
            var sb = new StringBuilder();
            foreach (var v in param.Value)
            {
                sb.Append(ULogManager.GetSimpleValue(v.Item1, v.Item2));
            }
            table.AddRow(new Markup($"[blue]{param.Key}[/]"), new Markup($"[red]{sb}[/]"));
        }
        AnsiConsole.Write(table);
    }
    
    public void ReadInfoMessages(string filePath)
    {
        var tokens = ReadTokens(filePath, token => token.TokenType == ULogToken.Information && token is ULogInformationMessageToken);
        var paramsDict = new Dictionary<string, IList<(ULogType, byte[])>>();

        foreach (var token in tokens.OfType<ULogInformationMessageToken>())
        {
            if (!paramsDict.TryGetValue(token.Key.Name, out var value))
            {
                value = new List<(ULogType, byte[])> { (token.Key.Type.BaseType, token.Value) };
                paramsDict[token.Key.Name] = value;
            }
            else
            {
                value.Add((token.Key.Type.BaseType, token.Value));
            }
        }
        AnsiConsole.MarkupLine($"[green]Total info messages read:[/] [bold red]{tokens.Count}[/]");
        AnsiConsole.MarkupLine("[blue]Info messages:[/]");
    
        foreach (var param in paramsDict)
        {
            var sb = new StringBuilder();
            foreach (var v in param.Value)
            {
                if (v.Item1 == ULogType.Char)
                {
                    sb.Append(CharToString(v.Item2).ToString());
                }
                else
                {
                    sb.Append(ULogManager.GetSimpleValue(v.Item1, v.Item2));
                }
            }
            AnsiConsole.MarkupLine($"[blue]{param.Key}[/]: [red]{sb.ToString().Replace("[", "[[").Replace("]", "]]")}[/]");
        }
    }
    
    public void ReadMessages(string filePath)
    {
        var tokens = ReadTokens(filePath, token => token.TokenType == ULogToken.LoggedString && token is ULogLoggedStringMessageToken);
        foreach (var token in tokens.OfType<ULogLoggedStringMessageToken>())
        {
            var time = new TimeSpan((long)token.TimeStamp * 10);
            AnsiConsole.MarkupLine($"[blue]{time:hh\\:mm\\:ss}[/]: [yellow]{token.LogLevel}[/] [green]{token.Message.Replace("[", "[[").Replace("]", "]]")}[/]");
        }
    }
    public void ReadSubscriptionsMessages(string filePath)
    {
        var tokens = ReadTokens(filePath, token => token.TokenType == ULogToken.Subscription && token is ULogSubscriptionMessageToken);
        var table = new Table();
        table.AddColumns("[blue]Name[/] ([yellow]multi id[/], [yellow]message size in bytes[/])", "[green]Value[/]");
        table.Border(TableBorder.Double);

        foreach (var token in tokens.OfType<ULogSubscriptionMessageToken>())
        {
            table.AddRow(new Markup($"[blue]{token.MessageName}[/] ([yellow]{token.MultiId}[/], [yellow]{token.GetByteSize()}[/])"), 
                new Markup($"{token.MessageId}"));
        }
        AnsiConsole.Write(table);
    }
    
    public void ReadLoggedDataById(string filePath, int msgId)
    {
        var tokens = ReadTokens(filePath, token => token.TokenType == ULogToken.LoggedData && token is ULogLoggedDataMessageToken loggedData && loggedData.MessageId == msgId);
        foreach (var token in tokens.OfType<ULogLoggedDataMessageToken>())
        {
            AnsiConsole.WriteLine($"{Convert.ToBase64String(token.Data)}", new Markup($"{token.MessageId}"));
        }
        AnsiConsole.MarkupLine($"[green]Total logged data messages for ID {msgId}:[/] [bold red]{tokens.Count}[/]");
    }
    
    private SequenceReader<byte> CreateSequenceReader(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var data = new ReadOnlySequence<byte>(fileBytes);
        return new SequenceReader<byte>(data); 
    }
    
    private ReadOnlySpan<char> CharToString(byte[] value)
    {
        var charSize = ULogManager.Encoding.GetCharCount(value);
        var charBuffer = new char[charSize];
        ULogManager.Encoding.GetChars(value,charBuffer);
        var rawString = new ReadOnlySpan<char>(charBuffer, 0, charSize);
        return rawString.ToString();
    }
    
    private List<IULogToken> ReadTokens(string filePath, Func<IULogToken, bool> filter)
    {
        var rdr = CreateSequenceReader(filePath);
        var reader = ULogManager.CreateReader();
        var tokens = new List<IULogToken>();

        while (reader.TryRead(ref rdr, out var token))
        {
            if (filter(token))
            {
                tokens.Add(token);
            }
        }

        return tokens;
    }
}