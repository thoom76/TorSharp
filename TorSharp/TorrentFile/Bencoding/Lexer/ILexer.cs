namespace TorSharp.TorrentFile.Bencoding.Lexer;

public interface ILexer
{
    public DictionaryToken GetTokens(string input);
}
