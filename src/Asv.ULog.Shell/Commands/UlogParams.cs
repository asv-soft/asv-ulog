using System.ComponentModel;
using Demo.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Asv.ULog.Shell;

[Description("Extract parameters from an ULog file")]
public class UlogParams : Command<UlogParams.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<FILE>")]
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
        //TODO: Functional method
        return 0;
        
    }
}