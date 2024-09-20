using TorSharp.TorrentFile.Models.FileTree;

namespace TorSharp.TorrentFile.Models.Info;

public interface IInfo
{
    public string TorrentName { get; }
    public long PieceBytes { get; }
    public long MetaVersion { get; }
    public IFileTree FileTree { get; }
}
