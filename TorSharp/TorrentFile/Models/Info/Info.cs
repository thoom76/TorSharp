using TorSharp.TorrentFile.Models.FileTree;

namespace TorSharp.TorrentFile.Models.Info;

public record Info(string TorrentName, long PieceBytes, long MetaVersion, IFileTree FileTree) : IInfo;