namespace TorSharp.PubSubService;

public interface IPubSubService<T> : IDisposable
{
    Subscription<T> SubscribeAsync(CancellationToken ctx = default);
    Task PublishAsync(T message, CancellationToken ctx = default);
}