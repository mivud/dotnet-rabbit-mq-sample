using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace EventBusRabbitMQ
{
    public class RabbitMQEventBus : IEventBus
    {
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQEventBus> _logger;

        private IChannel? _channel;

        public RabbitMQEventBus(IConnection connection,
            ILogger<RabbitMQEventBus> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        private async Task<IChannel> GetChannelAsync()
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _channel = await _connection.CreateChannelAsync();
            }

            return _channel;
        }

        public async Task PublishAsync<T>(string queue, T message)
             where T : IntegrationEvent
        {
            var channel = await GetChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queue,
                body: body);

            _logger.LogInformation(
                "----- [RabbitMQ] Queue {queue}: {id} published",
                queue,
                message.Id);
        }

        public async Task ConsumeAsync<T>(string queue, Func<T, Task> handler)
            where T : IntegrationEvent
        {
            var channel = await GetChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var obj = JsonSerializer.Deserialize<T>(message);
                ArgumentNullException.ThrowIfNull(obj);

                await handler(obj);

                _logger.LogInformation(
                    "----- [RabbitMQ] Queue {queue}: {id} consumed",
                    queue,
                    obj.Id);
            };

            await channel.BasicConsumeAsync(
                queue: queue,
                autoAck: true,
                consumer: consumer);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null && _channel.IsOpen)
                await _channel.CloseAsync();

            if (_connection.IsOpen)
                await _connection.CloseAsync();

            GC.SuppressFinalize(this);
        }
    }
}
