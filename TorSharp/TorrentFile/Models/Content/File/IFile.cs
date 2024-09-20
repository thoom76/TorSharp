namespace TorSharp.TorrentFile.Models.Content;

public interface IFile : IContent
{
    public long Length { get; }
    public string? PiecesRoot { get; }
}
