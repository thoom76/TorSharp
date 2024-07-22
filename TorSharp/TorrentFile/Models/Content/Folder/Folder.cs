namespace TorSharp.TorrentFile.Models.Content.Folder;

public record Folder(string Name, HashSet<IContent> Contents) : IFolder;