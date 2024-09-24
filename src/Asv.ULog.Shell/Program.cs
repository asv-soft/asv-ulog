using Spectre.Console.Cli;

namespace Asv.ULog.Shell;

public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("ulog");

            config.AddCommand<UlogFileStatistic>("ulog_statistic")
                .WithDescription("Show statistics about the ULog file")
                .WithExample("ulog_statistic", "file.ulg");

            config.AddCommand<UlogInfo>("ulog_info")
                .WithDescription("Show information about the ULog file")
                .WithExample("ulog_info", "file.ulg");

            config.AddCommand<UlogMessages>("ulog_messages")
                .WithDescription("Display logged messages from a ULog file")
                .WithExample("ulog_messages", "file.ulg");

            config.AddCommand<UlogParams>("ulog_params")
                .WithDescription("Extract parameters from a ULog file")
                .WithExample("ulog_params", "file.ulg");
        });

        return app.Run(args);
    }
}