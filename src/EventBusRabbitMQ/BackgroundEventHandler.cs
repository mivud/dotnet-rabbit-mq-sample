using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.Hosting;

namespace EventBusRabbitMQ
{
    public abstract class BackgroundEventHandler<T> : BackgroundService, IEventHandler<T>
        where T : IntegrationEvent
    {
        private readonly IEventBus _event;

        protected BackgroundEventHandler(IEventBus @event)
        {
            _event = @event;
        }

        public Task ConsumeAsync(string queue, Func<T, Task> handler)
        {
            return _event.ConsumeAsync(queue, handler);
        }
    }
}
