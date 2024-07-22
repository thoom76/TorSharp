using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TorSharp.PubSubService.InMemoryPubSub;

public class InMemoryPubSubService<T> : IPubSubService<T>
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Channel<T> _channel;
    private readonly ConcurrentDictionary<Guid, Func<T, CancellationToken, Task>> _subscribers;
    private readonly Task _processingTask;

    public InMemoryPubSubService()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _channel = Channel.CreateUnbounded<T>();
        _subscribers = new ConcurrentDictionary<Guid, Func<T, CancellationToken, Task>>();
        _processingTask = StartProcessing(_cancellationTokenSource.Token);
    }

    public Subscription<T> SubscribeAsync(CancellationToken ctx = default)
    {
        var id = Guid.NewGuid();

        var channel = Channel.CreateUnbounded<T>();
        _subscribers[id] = async (message, ctx) => await channel.Writer.WriteAsync(message, ctx);
        return new Subscription<T>(channel.Reader, () => Unsubscribe(id));
    }

    public async Task PublishAsync(T message, CancellationToken ctx = default)
    {
        await _channel.Writer.WriteAsync(message, ctx);
    }

    private void Unsubscribe(Guid subscriberId)
    {
        _subscribers.Remove(subscriberId, out _);
    }

    private async Task StartProcessing(CancellationToken ctx)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(ctx))
        {
            Parallel.ForEach(_subscribers.Values, async subscriber => await subscriber(message, ctx));
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
        _cancellationTokenSource.Cancel();
        _processingTask.Wait();
    }
}