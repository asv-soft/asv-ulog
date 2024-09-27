using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Asv.ULog.Shell;

[Description("Display information from an ULog file")]
public sealed class UlogInfo : Command<UlogInfo.Settings>
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
        var rdr = new UlogFileReader();
        rdr.ReadInfoMessages(settings.FilePath); 
        rdr.ReadSubscriptionsMessages(settings.FilePath);
        var msgIdToShow = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow][[Optional]][/]: Which subscription message to display? ")
                .AllowEmpty());
        rdr.ReadLoggedDataById(settings.FilePath, Int32.Parse(msgIdToShow));
        return 0;
    }
}