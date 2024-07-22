using System.Diagnostics.CodeAnalysis;
using TorSharp.TorrentFile.Models;
using TorSharp.TorrentFile.Models.Content;
using TorSharp.TorrentFile.Models.Content.Folder;
using TorSharp.TorrentFile.Models.FileTree;
using TorSharp.TorrentFile.Models.Info;

namespace TorSharp.TorrentFile.Bencoding.Parser;

public interface IParser
{
    IInfo GetInfo(DictionaryToken infoToken);
    HashSet<IContent> GetContents(DictionaryToken contentsToken);
    IFileTree GetFileTree(DictionaryToken fileTreeToken);
    bool TryGetFolderFromDictionaryToken(StringToken nameToken, DictionaryToken dictionaryToken, [NotNullWhen(true)] out IFolder? folder, [NotNullWhen(false)] out IList<string>? issues);
    bool TryGetFileFromDictionaryToken(StringToken nameToken, DictionaryToken dictionaryToken, [NotNullWhen(true)] out IFile? file, [NotNullWhen(false)] out IList<string>? issues);
    ITorrentMetadata GetTorrentMetadata(DictionaryToken tokens);
}
