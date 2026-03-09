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
        private readonly ConnectionFactory _factory;
        private readonly ILogger<RabbitMQEventBus> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQEventBus(ConnectionFactory factory,
            ILogger<RabbitMQEventBus> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        private async Task<IConnection> GetConnectionAsync()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = await _factory.CreateConnectionAsync();
            }

            return _connection;
        }

        private async Task<IChannel> GetChannelAsync()
        {
            if (_channel == null || !_channel.IsOpen)
            {
                var connection = await GetConnectionAsync();
                _channel = await connection.CreateChannelAsync();
            }

            return _channel;
        }

        public async Task PublishAsync<T>(string queue, T message)
             where T : IntegrationEvent
        {
            var channel = await GetChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queue,
                mandatory: false,
                basicProperties: props,
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
                durable: true,
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
            if (_channel?.IsOpen is true)
                await _channel.CloseAsync();

            if (_connection?.IsOpen is true)
                await _connection.CloseAsync();

            GC.SuppressFinalize(this);
        }
    }
}
