using TorSharp.TorrentFile.Models.Content;

namespace TorSharp.TorrentFile.Models.FileTree;

public record FileTree(HashSet<IContent> Contents) : IFileTree;