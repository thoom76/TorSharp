namespace TorSharp.TorrentFile.Models.Content.Folder;

public interface IFolder : IContent
{
    public HashSet<IContent> Contents { get; }
}