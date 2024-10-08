using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Asv.ULog.Shell;

[Description("Display logged messages from an ULog file")]
public class UlogMessages : Command<UlogMessages.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<[File path]>")]
        [Description("Path to the ULog file.")]
        public string FilePath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!File.Exists(settings.FilePath))
        {
            AnsiConsole.Markup("[red]Error: File not found![/]");
            return -1;
        }
        new UlogFileReader().ReadMessages(settings.FilePath);
        return 0;
        
    }
}