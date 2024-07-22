using TorSharp.TorrentFile.Models;

namespace TorSharp.TorrentFile.Services;

public interface ITorrentFileService
{
    public ITorrentMetadata GetTorrentMetadata(FileInfo filePath);

    public ITorrentMetadata GetTorrentMetadata(ReadOnlySpan<byte> torrentData);

}
