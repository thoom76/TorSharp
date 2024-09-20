namespace TorSharp.TorrentFile.Models.Content;

public record File(
    string Name,
    long Length,
    string PiecesRoot // It can only be nullable for the fact that an empty file does not contain a root (since there is no data).
) : IFile;
