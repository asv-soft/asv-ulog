using System.Buffers;
using Spectre.Console;

namespace Asv.ULog.Shell;

public class UlogFileReader
{
    public void ReadULogFile(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var data = new ReadOnlySequence<byte>(fileBytes);
        var rdr = new SequenceReader<byte>(data); 
        var reader = ULog.CreateReader();
        int index = 0;
        var stat = Enum.GetValues<ULogToken>().ToDictionary(token => token, token => 0);
        while (reader.TryRead(ref rdr, out var token))
        {
            stat[token.TokenType] += 1;
            index++;
        } 
        AnsiConsole.Markup($"[green]Total tokens read:[/] [bold red]{index}[/]\n");
        var table = new Table();
        table.AddColumn("Tokens");
        table.AddColumn(new TableColumn("Count").Centered());
        table.Border(TableBorder.Double);
        foreach (var (key, value) in stat)
        {
            table.AddRow(new Markup($"[blue]{key}[/]"), new Markup($"[green]{value}[/]"));
        }
        AnsiConsole.Write(table);
    }
}