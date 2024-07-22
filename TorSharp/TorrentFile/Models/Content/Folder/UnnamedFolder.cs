namespace TorSharp.TorrentFile.Models.Content.Folder;

public record UnnamedFolder(HashSet<IContent> Contents) : IFolder
{
    public string Name => string.Empty;
}
