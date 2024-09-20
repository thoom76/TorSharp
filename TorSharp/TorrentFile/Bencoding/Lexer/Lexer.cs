using TorSharp.TorrentFile.Bencoding.Lexer;

namespace TorSharp.TorrentFile.Lexer;

public class Lexer : ILexer
{
    public DictionaryToken GetTokens(string input)
    {
        var i = 0;
        var baseToken = Tokenize(input, ref i);
        if (baseToken is not DictionaryToken dictionaryToken)
        {
            throw new Exception($"The root token must be a dictionary token");
        }
        return dictionaryToken;
    }

    private static IntegerToken ReadInteger(string input, ref int i)
    {
        if (input[i] is not 'i')
        {
            throw new Exception($"Invalid integer token '{input}'");
        }
        string result = "";
        while (++i < input.Length)
        {
            if (input[i] is 'e')
            {
                i++;
                return new IntegerToken { Value = long.Parse(result) };
            }
            result += input[i];
        }

        throw new Exception($"Invalid integer token '{input[i..]}'");
    }

    private static StringToken ReadString(string input, ref int i)
    {
        string stringLengthString = "";
        string result = "";
        while (i < input.Length)
        {
            if (input[i] is not ':')
            {
                stringLengthString += input[i++];
                continue;
            }

            int stringLength = int.Parse(stringLengthString);
            while (i < input.Length && stringLength > 0)
            {
                result += input[++i];
                stringLength--;
            }
            i++;
            return new StringToken { Value = result };
        }

        throw new Exception($"Invalid string token '{input}'");
    }


    private static ListToken ReadList(string input, ref int i)
    {
        if (input[i++] is not 'l')
        {
            throw new Exception($"Invalid list token '{input}'");
        }
        List<BaseToken> tokens = new();
        while (i < input.Length)
        {
            if (input[i] is 'e')
            {
                i++;
                break;
            }
            tokens.Add(Tokenize(input, ref i));
        }
        return new ListToken { Value = tokens };
    }

    private static DictionaryToken ReadDictionary(string input, ref int i)
    {
        if (input[i++] is not 'd')
        {
            throw new Exception($"Invalid dictionary token '{input}'");
        }
        Dictionary<StringToken, BaseToken> tokens = new();
        while (i < input.Length)
        {
            if (input[i] is 'e')
            {
                i++;
                break;
            }
            var key = ReadString(input, ref i);
            var value = Tokenize(input, ref i);
            tokens.Add(key, value);
        }
        return new DictionaryToken { Value = tokens };
    }

    private static BaseToken Tokenize(string input, ref int i)
    {
        var token = input[i] switch
        {
            'i' => (ReadInteger(input, ref i) as BaseToken)!,
            'l' => (ReadList(input, ref i) as BaseToken)!,
            'd' => (ReadDictionary(input, ref i) as BaseToken)!,
            >= '0' and <= '9' => (ReadString(input, ref i) as BaseToken)!,
            _ => throw new Exception($"Invalid start of new token: '{input[i]}'"), 
        };
        return token;
    }
}