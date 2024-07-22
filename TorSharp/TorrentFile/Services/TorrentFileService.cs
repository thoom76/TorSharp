using TorSharp.TorrentFile.Models;

namespace TorSharp.TorrentFile.Services;

public class TorrentFileService : ITorrentFileService
{
    public TorrentFileService()
    {
    }

    public ITorrentMetadata GetTorrentMetadata(FileInfo filePath)
    {
        throw new NotImplementedException();
    }

    public ITorrentMetadata GetTorrentMetadata(ReadOnlySpan<byte> torrentData)
    {
        throw new NotImplementedException();
    }

}