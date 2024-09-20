public abstract class BaseToken
{
    public abstract override string ToString();
}

public abstract class BaseToken<TValue> : BaseToken
{
    public required TValue Value { get; init; }
}

public class IntegerToken : BaseToken<long>
{
    public override string ToString() => Value.ToString();
}

public class StringToken : BaseToken<string>
{
    public override string ToString() => $"'{Value}'";
}

public class ListToken : BaseToken<List<BaseToken>>
{
    public override string ToString() => $"[{string.Join(", ", Value)}]";
}

public class DictionaryToken : BaseToken<Dictionary<StringToken, BaseToken>>
{
    public override string ToString() => $"{{{string.Join(", ", Value.Select(kv => $"{kv.Key}: {kv.Value}"))}}}";
}