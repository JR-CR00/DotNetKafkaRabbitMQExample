using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Messaging.RabbitMQ;

public abstract class RabbitMQConsumerBase : BackgroundService
{
    protected readonly IConfiguration _config;
    protected readonly IServiceScopeFactory _scopeFactory;
    protected readonly ILogger<RabbitMQConsumerBase> _logger;
    protected abstract string TargetQueue { get; }
    protected virtual int MaxRetries => 3;

    public RabbitMQConsumerBase(
        IConfiguration config, 
        IServiceScopeFactory scopeFactory, 
        ILogger<RabbitMQConsumerBase> logger)
    {
        _config = config;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() => StartConsuming(stoppingToken), stoppingToken);
    }

    private async Task StartConsuming(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "localhost",
            UserName = _config["RabbitMQ:Username"] ?? "admin",
            Password = _config["RabbitMQ:Password"] ?? "admin123"
        };

        try
        {
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var retryCount = GetRetryCount(ea);
                
                bool success = false;
                
                using (var scope = _scopeFactory.CreateScope())
                {
                    try
                    {
                        success = await ProcessMessageAsync(body, scope.ServiceProvider);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando mensaje en la cola {TargetQueue}", TargetQueue);
                        success = false;
                    }
                }

                if (success)
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                else if (retryCount < MaxRetries)
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
                    _logger.LogWarning("Fallo en {TargetQueue}, reintentando... ({retryCount}/{MaxRetries})", TargetQueue, retryCount + 1, MaxRetries);
                }
                else
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                    _logger.LogError("Max reintentos superados en {TargetQueue} -> DLQ", TargetQueue);
                }
            };

            await channel.BasicConsumeAsync(queue: TargetQueue, autoAck: false, consumer: consumer);

            _logger.LogInformation("Escuchando cola: {TargetQueue}", TargetQueue);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error fatal en el consumidor de la cola {TargetQueue}", TargetQueue);
        }
    }

    protected abstract Task<bool> ProcessMessageAsync(string body, IServiceProvider serviceProvider);

    private static int GetRetryCount(BasicDeliverEventArgs ea)
    {
        if (ea.BasicProperties.Headers != null &&
            ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryObj) &&
            retryObj is int retryCount)
            return retryCount;
        return 0;
    }
}


