using TorSharp.TorrentFile.Models.FileTree;

namespace TorSharp.TorrentFile.Models.Info;

public record Info(string TorrentName, int PieceBytes, int MetaVersion, IFileTree FileTree) : IInfo;