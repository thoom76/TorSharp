using TorSharp.TorrentFile.Models.Info;

namespace TorSharp.TorrentFile.Models;

public record TorrentMetadata(string Announce, IInfo Info, Dictionary<string, string> PieceLayers) : ITorrentMetadata;