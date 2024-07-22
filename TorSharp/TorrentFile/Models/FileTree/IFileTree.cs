using TorSharp.TorrentFile.Models.Content;

namespace TorSharp.TorrentFile.Models.FileTree;

public interface IFileTree
{
    public HashSet<IContent> Contents { get; }
}
