using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Serilog;
using Spectre.Console;
using TorSharp.BackgroundServices.UIService.AnsiConsoleExtensions;
using TorSharp.PubSubService;

namespace TorSharp.BackgroundServices.UIService;

public enum NotificationType {
    INFO,
    WARNING,
    ERROR
}
public record Notification(NotificationType Type, string Content); 
public record NotificationSent(Notification Message) : BaseEvent<Notification>;

internal class UIState{
    public Queue<Notification> Notifications = new();
}

public sealed class UIService(
    ILogger<UIService> logger,
    IPubSubService<TorrentMetadataDownloadRequested> downloadRequestedPubSubService,
    IPubSubService<NotificationSent> notificationSentPubSubService
) : BackgroundService
{
    private const int TargetFPS = 10;
    private const int FrameMS = 1000 / TargetFPS;
    private IAnsiConsole Terminal => AnsiConsole.Create(new AnsiConsoleSettings
    {
        Ansi = AnsiSupport.Yes
    });
    private readonly UIState state = new(); 

    private int progress = 0;

    private async Task BackgroundNotificationHandler(CancellationToken ctx){
        var notificationSentSubscription = notificationSentPubSubService.SubscribeAsync(ctx);
        await Task.Run(async () => {
            await foreach(var message in notificationSentSubscription.Messages){
                logger.LogInformation("Received notification: {Notification}", message.Message);
                lock(state.Notifications){
                    state.Notifications.Enqueue(message.Message);
                }
            }
        }, ctx);
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("UI service is starting.");
        
        var notificationSyncTask = BackgroundNotificationHandler(ctx); 

        var stopwatch = new Stopwatch();
        while (!ctx.IsCancellationRequested)
        {
            stopwatch.Restart();

            await Update(ctx);
            Render();

            await Task.Delay(Math.Max(0, FrameMS - (int)stopwatch.ElapsedMilliseconds), ctx);
        }

        logger.LogInformation("UI service is stopping.");

        await notificationSyncTask;
    }

    private async Task Update(CancellationToken ctx = default)
    {
        progress = (progress + 1) % 100;

        if(Console.KeyAvailable)
        {
            var input = Console.ReadKey(true);
            var key = input.Key;
            var modifiers = input.Modifiers;

            if(state.Notifications.FirstOrDefault() is not null){
                if(key == ConsoleKey.Enter){
                    state.Notifications.Dequeue();
                }
                return;
            } 
            
            if(key == ConsoleKey.D)
            {
                // TODO: Should open the downloads page 
                return;
            }
            if(key == ConsoleKey.T)
            {
                // TODO: Should open the download torrent page
                await downloadRequestedPubSubService.PublishAsync(new TorrentMetadataDownloadRequested("https://libtorrent.org/bittorrent-v2-test.torrent"), ctx);
                return;
            }
            if(key == ConsoleKey.Q)
            {
                Environment.Exit(0);
                return;
            }
        }

    }

    private void Render()
    {
        Terminal.Clear();

        var notification = state.Notifications.FirstOrDefault();
        if(notification is not null){
            Terminal.MarkupLine($"\n\n[bold][red]{notification.Type}: {notification.Content}[/][/]");
            Terminal.MarkupLine($"\n\n[bold][#757575]Coninue: <Enter> [/][/]");
            return;
        }

        Terminal
            .progressView()
            .AddProgressBar("[bold green]Torrent metadata[/]", progress)
            .AddProgressBar("[bold yellow]Torrent downloading in progressss[/]", 100)
            .Render();

        
        Terminal.MarkupLine($"\n\n[bold][#757575]Exit: <Ctrl+Q> ¦ Downloads: <Ctrl+D> ¦ Download Torrent: <Ctrl+T>[/][/]");
    }
}
