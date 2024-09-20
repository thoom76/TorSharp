namespace TorSharp.TorrentFile.Bencoding.Parser;

public record TokenNameGraph(params string[] recursiveTokenNames){
    public override string ToString() => $"'{string.Join("' -> '", recursiveTokenNames)}'";
}