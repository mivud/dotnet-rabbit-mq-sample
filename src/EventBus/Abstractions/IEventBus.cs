using EventBus.Events;

namespace EventBus.Abstractions
{
    public interface IEventBus : IAsyncDisposable
    {
        Task PublishAsync<T>(string queue, T message)
            where T : IntegrationEvent;

        Task ConsumeAsync<T>(string queue, Func<T, Task> handler)
            where T : IntegrationEvent;
    }
}
