using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.Hosting;

namespace EventBusRabbitMQ
{
    public abstract class HostedEventHandler<T> : IHostedService, IEventHandler<T>
        where T : IntegrationEvent
    {
        private readonly IEventBus _event;

        protected HostedEventHandler(IEventBus @event)
        {
            _event = @event;
        }

        public Task ConsumeAsync(string queue, Func<T, Task> handler)
        {
            return _event.ConsumeAsync(queue, handler);
        }

        public abstract Task StartAsync(CancellationToken cancellationToken);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _event.DisposeAsync();
        }
    }
}
