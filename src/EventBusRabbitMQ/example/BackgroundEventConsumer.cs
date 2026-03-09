using EventBus.Abstractions;

namespace EventBusRabbitMQ.example
{
    public class BackgroundEventConsumer : BackgroundEventHandler<EventData>
    {
        const string _queue = MessageQueue.Queue;

        public BackgroundEventConsumer(IEventBus @event) : base(@event)
        {
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return ConsumeAsync();
        }

        private Task ConsumeAsync()
        {
            return ConsumeAsync(_queue, async data =>
            {
                Console.WriteLine($"----- [RabbitMQ] Message received: {data.Id}");
                await WriteValueAsync(data);
            });
        }

        public static async Task WriteValueAsync(EventData data)
        {
            Console.WriteLine($"----- [RabbitMQ] Message received: {data.Message}");
            await Task.CompletedTask;
        }
    }
}
