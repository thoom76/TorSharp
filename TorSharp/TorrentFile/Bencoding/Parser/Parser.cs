using System.Diagnostics.CodeAnalysis;
using TorSharp.TorrentFile.Models;
using TorSharp.TorrentFile.Models.Content;
using TorSharp.TorrentFile.Models.Content.Folder;
using TorSharp.TorrentFile.Models.FileTree;
using TorSharp.TorrentFile.Models.Info;
using File = TorSharp.TorrentFile.Models.Content.File;

namespace TorSharp.TorrentFile.Bencoding.Parser;

public class Parser : IParser
{
    public class IssueFormater
    {
        public static string WrongValueType(TokenNameGraph tokenNameGraph, Type expectedType, Type receivedType)
        {
            return $"The token {tokenNameGraph} must be of type {expectedType.Name}, received: {receivedType.Name}";
        }

        public static string MissingToken(TokenNameGraph tokenNameGraph)
        {
            return $"The token {tokenNameGraph} is missing";
        }

        public static string WrongContentType(TokenNameGraph tokenNameGraph, IList<string> fileIssues, IList<string> folderIssues)
        {
            return $"The token {tokenNameGraph} must be of type {typeof(IFile).Name} or {typeof(IFolder).Name}. The following issues were found: <${nameof(fileIssues)}>:[{string.Join(", ", fileIssues)}] and <${nameof(folderIssues)}>:[{string.Join(", ", folderIssues)}]";
        }
    }

    public IInfo GetInfo(DictionaryToken infoToken)
    {
        var tokensCount = infoToken.Value.Count;

        DictionaryToken? fileTreeToken = null;
        StringToken? torrentNameToken = null;
        IntegerToken? pieceLengthToken = null;
        IntegerToken? metaVersionToken = null;

        List<string> issues = new();

        for (int i = 0; i < tokensCount; i++)
        {
            var keyValuePair = infoToken.Value.ElementAt(i);
            if (keyValuePair.Key.Value == "file tree")
            {
                if (keyValuePair.Value is not DictionaryToken dictionaryToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new ("info", "file tree"), typeof(DictionaryToken), keyValuePair.Value.GetType()));
                    continue;
                }
                fileTreeToken = dictionaryToken;
            }

            if (keyValuePair.Key.Value == "name")
            {
                if (keyValuePair.Value is not StringToken stringToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new ("info", "name"), typeof(StringToken), keyValuePair.Value.GetType()));
                    continue;
                }
                torrentNameToken = stringToken;
            }

            if (keyValuePair.Key.Value == "piece length")
            {
                if (keyValuePair.Value is not IntegerToken integerToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new ("info", "piece length"), typeof(IntegerToken), keyValuePair.Value.GetType()));
                    continue;
                }
                pieceLengthToken = integerToken;
            }

            if (keyValuePair.Key.Value == "meta version")
            {
                if (keyValuePair.Value is not IntegerToken integerToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new ("info", "meta version"), typeof(IntegerToken), keyValuePair.Value.GetType()));
                    continue;
                }
                if (integerToken.Value != 2)
                {
                    issues.Add($"The token {new TokenNameGraph("info", "meta version")} must be version '2'. Other versions are not supported");
                }
                metaVersionToken = integerToken;
            }
        }

        if (fileTreeToken is null)
        {
            issues.Add(IssueFormater.MissingToken(new ("info", "file tree")));
        }
        if (torrentNameToken is null)
        {
            issues.Add(IssueFormater.MissingToken(new ("info", "name")));
        }
        if (pieceLengthToken is null)
        {
            issues.Add(IssueFormater.MissingToken(new ("info", "piece length")));
        }
        if (metaVersionToken is null)
        {
            issues.Add(IssueFormater.MissingToken(new ("info", "meta version")));
        }

        if (issues.Count > 0)
        {
            throw new Exception(string.Join(", ", issues));
        }

        return new Info(torrentNameToken!.Value, pieceLengthToken!.Value, metaVersionToken!.Value, GetFileTree(fileTreeToken!));
    }

    public HashSet<IContent> GetContents(DictionaryToken contentsToken)
    {
        HashSet<IContent> contents = new();

        List<string> issues = new();
        foreach (var kv in contentsToken.Value)
        {
            if (kv.Value is not DictionaryToken dictionaryToken)
            {
                issues.Add(IssueFormater.WrongValueType(new (kv.Key.Value), typeof(DictionaryToken), kv.Value.GetType()));
                continue;
            }

            if (TryGetFileFromDictionaryToken(kv.Key, dictionaryToken, out var file, out var fileIssues))
            {
                contents.Add(file!);
                continue;
            }

            if (TryGetFolderFromDictionaryToken(kv.Key, dictionaryToken, out var folder, out var folderIssues))
            {
                contents.Add(folder!);
                continue;
            }

            issues.Add(IssueFormater.WrongContentType(new (kv.Key.Value), fileIssues, folderIssues));
        }

        return contents;
    }

    public IFileTree GetFileTree(DictionaryToken fileTreeToken)
    {
        return new FileTree(GetContents(fileTreeToken));
    }

    public bool TryGetFolderFromDictionaryToken(StringToken nameToken, DictionaryToken dictionaryToken, [NotNullWhen(true)] out IFolder? folder, [NotNullWhen(false)] out IList<string>? issues)
    {
        issues = [];
        folder = nameToken.Value switch {
            string name when name.Length > 0 => new Folder(name, GetContents(dictionaryToken!)),
            _ => new UnnamedFolder(GetContents(dictionaryToken!)),
        };
        return true;
    }

    public bool TryGetFileFromDictionaryToken(StringToken nameToken, DictionaryToken dictionaryToken, [NotNullWhen(true)] out IFile? file, [NotNullWhen(false)] out IList<string>? issues)
    {
        var tokensCount = dictionaryToken.Value.Count;

        IntegerToken? lengthToken = null;
        StringToken? piecesRootToken = null;

        issues = [];

        for (int i = 0; i < tokensCount; i++)
        {
            var token = dictionaryToken.Value.ElementAt(i);
            if (token.Key.Value == "length")
            {
                if (token.Value is not IntegerToken integerToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new (nameToken.Value, "length"), typeof(IntegerToken), token.Value.GetType()));
                    continue;
                }
                lengthToken = integerToken;
            }

            if (token.Key.Value == "pieces root")
            {
                if (token.Value is not StringToken stringToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new (nameToken.Value, "pieces root"), typeof(StringToken), token.Value.GetType()));
                    continue;
                }
                piecesRootToken = stringToken;
            }
        }

        if (lengthToken is null)
        {
            issues.Add(IssueFormater.MissingToken(new (nameToken.Value, "length")));
        }

        if (issues.Count > 0)
        {
            file = null;
            return false;
        }

        file = (lengthToken!.Value, piecesRootToken!.Value) switch {
            (var length, var piecesRoot) when length > 0 && piecesRoot is not null => new File(nameToken.Value, length, piecesRoot),
            (var length, var piecesRoot) when length == 0 && piecesRoot is null => new EmptyFile(nameToken.Value),
            _ => null, 
        };


        if (file is null)
        {
            issues.Add("The file must have a length greater than 0 and a pieces root");
            return false;
        }

        issues = null;
        return true; 
    }

    public ITorrentMetadata GetTorrentMetadata(DictionaryToken tokens)
    {
        var tokensCount = tokens.Value.Count;

        StringToken? announce = null;
        DictionaryToken? info = null;
        DictionaryToken? pieceLayers = null;

        List<string> issues = new();

        for (int i = 0; i < tokensCount; i++)
        {
            var token = tokens.Value.ElementAt(i);

            if (token.Key.Value == "announce")
            {
                if (token.Value is not StringToken announceToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new ("announce"), typeof(StringToken), token.Value.GetType()));
                    continue;
                }
                announce = announceToken;
            }

            if (token.Key.Value == "info")
            {
                if (token.Value is not DictionaryToken infoToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new ("info"), typeof(DictionaryToken), token.Value.GetType()));
                    continue;
                }
                info = infoToken;
            }

            if (token.Key.Value == "piece layers")
            {
                if (token.Value is not DictionaryToken pieceLayersToken)
                {
                    issues.Add(IssueFormater.WrongValueType(new ("piece layers"), typeof(DictionaryToken), token.Value.GetType()));
                    continue;
                }
                pieceLayers = pieceLayersToken;
            }
        }

        if (announce is null)
        {
            issues.Add(IssueFormater.MissingToken(new ("announce")));
        }
        if (info is null)
        {
            issues.Add(IssueFormater.MissingToken(new ("info")));
        }
        if (pieceLayers is null)
        {
            issues.Add(IssueFormater.MissingToken(new("piece layers")));
        }

        if (info?.Value.FirstOrDefault(kv => kv.Key.Value == "meta version").Value is not IntegerToken metaVersionToken || metaVersionToken.Value != 2)
        {
            issues.Add($"The token {new TokenNameGraph("info", "meta version")} must be version 2; other versions are not supported (yet)");
        }

        if (issues.Count > 0)
        {
            throw new Exception($"Errors while parsing the tokens:\n\t{string.Join("\n\t", issues)}");
        }

        return new TorrentMetadata(announce!.Value, GetInfo(info!), GetPieceLayers(pieceLayers!));
    }

    public Dictionary<string, string> GetPieceLayers(DictionaryToken pieceLayersToken)
    {
        List<string> invalidTokens = new();
        Dictionary<string, string> pieceLayers = new();
        foreach (var kv in pieceLayersToken.Value)
        {
            if (kv.Value is not StringToken piecesLayerValue)
            {
                invalidTokens.Add(kv.Key.Value);
                continue;
            }
            pieceLayers.Add(kv.Key.Value, piecesLayerValue.Value);
        }
        return pieceLayers;
    }
}