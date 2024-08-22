using System.Net.NetworkInformation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TorSharp.PubSubService;
using TorSharp.TorrentFile.Models;

namespace TorSharp.BackgroundServices;

public sealed class TorrentDownloaderService(
    ILogger<TorrentDownloaderService> logger,
    IPubSubService<TorrentDownloadRequested> pubSubService
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("TorSharp service is starting.");

        using var subscription = pubSubService.SubscribeAsync(ctx);
        await foreach (var downloadRequestedEvent in subscription.Messages)
        {
            logger.LogDebug("Received message: {message}", downloadRequestedEvent);
            if(downloadRequestedEvent is null){
                continue;
            }

            var torrentMetadata = downloadRequestedEvent.Message;

            // TODO: Download the files with the torrent metadata.
            logger.LogDebug("Torrent announce URL: {Announce}", torrentMetadata.Announce);
        }

        logger.LogInformation("TorSharp service is stopping.");
    }
}