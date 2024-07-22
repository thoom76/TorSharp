using TorSharp.TorrentFile.Models.FileTree;

namespace TorSharp.TorrentFile.Models.Info;

public interface IInfo
{
    public string TorrentName { get; }
    public int PieceBytes { get; }
    public int MetaVersion { get; }
    public IFileTree FileTree { get; }
}
