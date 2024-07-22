namespace TorSharp.TorrentFile.Models.Content;

public interface IFile : IContent
{
    public int Length { get; }
    public string? PiecesRoot { get; }
}
