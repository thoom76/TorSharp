using TorSharp.TorrentFile.Models.Info;

namespace TorSharp.TorrentFile.Models;

public interface ITorrentMetadata
{
    public string Announce { get; }
    public IInfo Info { get; }
    public Dictionary<string, string> PieceLayers { get; }
}
