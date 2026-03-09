using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace EventBusRabbitMQ;

public static class Startup
{
    public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            //Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
            var factory1 = new ConnectionFactory
            {
                HostName = "10.32.183.166",
                UserName = "storeuser",
                Password = "CHANGE_ME",
                VirtualHost = "/",
                Port = 5672,
                Ssl = new SslOption
                {
                    Enabled = false
                }
            };

            var factory = new ConnectionFactory
            {
                HostName = "10.114.32.16",
                UserName = "super",
                Password = "adm!n"
            };

            //Create the RabbitMQ connection using connection factory details as i mentioned above
            return factory.CreateConnectionAsync().Result;
        });

        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        return services;
    }

    public static void Subscribe<TMessage, TEventHandler>(this IServiceCollection services)
        where TMessage : IntegrationEvent
        where TEventHandler : class, IEventHandler<TMessage>, IHostedService
    {
        services.AddHostedService<TEventHandler>();
    }
}