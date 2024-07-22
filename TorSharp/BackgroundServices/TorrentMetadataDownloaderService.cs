using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TorSharp.PubSubService;
using TorSharp.TorrentFile.Bencoding.Lexer;
using TorSharp.TorrentFile.Bencoding.Parser;

namespace TorSharp.BackgroundServices;

public sealed class TorrentMetadataDownloaderService(
    ILogger<TorrentMetadataDownloaderService> logger,
    IPubSubService<TorrentMetadataDownloadRequested> torrentMetadataDownloadPubSub,
    IPubSubService<TorrentDownloadRequested> torrentDownloadPubSub,
    ILexer lexer,
    IParser parser
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("Torrent metadata downloader service is starting.");

        using var subscription = torrentMetadataDownloadPubSub.SubscribeAsync(ctx);
        await foreach (var message in subscription.Messages)
        {
            logger.LogDebug("Received message: {message}", message);

            // TODO: Download the torrent file from the URL in the message
            var tokens = lexer.GetTokens("d8:announce35:http://tracker.example.com/announce7:comment32:This is an example torrent file.10:created by25:ExampleTorrentCreator 2.013:creation datei1625563200e4:infod11:file lengthi100000000e9:file treed9:file1.txtd6:lengthi12345678e11:pieces root18:<binary hash data>e9:file2.txtd6:lengthi87654321e11:pieces root18:<binary hash data>ee12:meta versioni2e4:name14:example_folder12:piece lengthi262144e11:pieces root18:<binary hash data>e12:piece layersd16:<some data hash>19:<another data hash>ee");
            var torrentMetadata = parser.GetTorrentMetadata(tokens);

            // TODO: Don't call PublishAsync directly, use a separate service to download the torrent files
            await torrentDownloadPubSub.PublishAsync(new TorrentDownloadRequested(torrentMetadata));
        }

        logger.LogInformation("Torrent metadata downloader service is stopping.");
    }
}