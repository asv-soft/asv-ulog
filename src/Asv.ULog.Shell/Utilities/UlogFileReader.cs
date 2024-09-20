using System.Buffers;
using Asv.IO;
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
        AnsiConsole.WriteLine($"Read {index} tokens");
        foreach (var (key, value) in stat)
        {
            AnsiConsole.WriteLine($"{key} : {value}");
        }
    }
}