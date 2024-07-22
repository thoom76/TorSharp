namespace TorSharp.TorrentFile.Models.Content;

public record EmptyFile(string Name) : IFile
{
    public int Length => 0;
    public string? PiecesRoot => null;
}
