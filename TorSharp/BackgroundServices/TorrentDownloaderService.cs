using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TorSharp.PubSubService;
using TorSharp.TorrentFile.Bencoding.Parser;
using TorSharp.TorrentFile.Lexer;

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
        await foreach (var nullableEvent in subscription.Messages)
        {
            logger.LogDebug("Received message: {message}", nullableEvent);
            if(nullableEvent is null){
                continue;
            }

            var torrentMetadata = nullableEvent.Message;

            // TODO: Download the files with the torrent metadata.
            logger.LogDebug("Torrent announce URL: {Announce}", torrentMetadata.Announce);
        }

        logger.LogInformation("TorSharp service is stopping.");
    }
}