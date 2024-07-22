using System.Threading.Channels;

namespace TorSharp.PubSubService;

public class Subscription<T> : IDisposable
{
    private readonly ChannelReader<T> _reader;
    private readonly Action _unsubscribe;

    public IAsyncEnumerable<T> Messages => _reader.ReadAllAsync();

    public Subscription(ChannelReader<T> reader, Action unsubscribe)
    {
        _reader = reader;
        _unsubscribe = unsubscribe;
    }

    public void Dispose()
    {
        _unsubscribe();
    }
}