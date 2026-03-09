using EventBus.Events;

namespace EventBusRabbitMQ
{
    public interface IEventHandler<T>
         where T : IntegrationEvent
    {
        Task ConsumeAsync(string queue, Func<T, Task> handler);
    }
}
