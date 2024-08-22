using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using TorSharp.BackgroundServices.UIService.AnsiConsoleExtensions;
using TorSharp.PubSubService;

namespace TorSharp.BackgroundServices.UIService;

public sealed class UIService(
    ILogger<UIService> logger,
    IPubSubService<TorrentMetadataDownloadRequested> pubSubService
) : BackgroundService
{
    private const int TargetFPS = 10;
    private const int FrameMS = 1000 / TargetFPS;
    private IAnsiConsole Terminal => AnsiConsole.Create(new AnsiConsoleSettings
    {
        Ansi = AnsiSupport.Yes
    });

    private int progress = 0;

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("UI service is starting.");

        var stopwatch = new Stopwatch();

        while (!ctx.IsCancellationRequested)
        {
            stopwatch.Restart();

            await Update();
            Render();

            Thread.Sleep(Math.Max(0, FrameMS - (int)stopwatch.ElapsedMilliseconds));
        }

        logger.LogInformation("UI service is stopping.");
    }

    private async Task Update()
    {
        progress = (progress + 1) % 100;
        if(Console.KeyAvailable)
        {
            var input = Console.ReadKey(true);
            var key = input.Key;
            var modifiers = input.Modifiers;

            if(modifiers == ConsoleModifiers.Control)
            {
                if(key == ConsoleKey.D)
                {
                    // TODO: Should open the downloads page 
                    return;
                }
                if(key == ConsoleKey.T)
                {
                    // TODO: Should open the download torrent page
                    await pubSubService.PublishAsync(new TorrentMetadataDownloadRequested("https://libtorrent.org/bittorrent-v2-test.torrent"));
                    return;
                }
                if(key == ConsoleKey.Q)
                {
                    Environment.Exit(0);
                    return;
                }
            }
        }

    }

    private void Render()
    {
        Terminal.Clear();

        Terminal
            .progressView()
            .AddProgressBar("[bold green]Torrent metadata[/]", progress)
            .AddProgressBar("[bold yellow]Torrent downloading in progressss[/]", 100)
            .Render();
        
        Terminal.MarkupLine($"\n\n[bold][#757575]Exit: <Ctrl+Q> ¦ Downloads: <Ctrl+D> ¦ Download Torrent: <Ctrl+T>[/][/]");
    }
}
