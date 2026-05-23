using System.Text.Json;
using DotNetKafkaRabbitMQExample.Application.Events;
using DotNetKafkaRabbitMQExample.Application.Interfaces;
using DotNetKafkaRabbitMQExample.Infrastructure.Messaging.Kafka;
using DotNetKafkaRabbitMQExample.Infrastructure.Messaging.RabbitMQ;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Workers;

public class TelegramWorkerService : RabbitMQConsumerBase
{
    protected override string TargetQueue => RabbitMQQueue.NotificationsTelegram;

    public TelegramWorkerService(
        IConfiguration config, 
        IServiceScopeFactory scopeFactory, 
        ILogger<TelegramWorkerService> logger) 
        : base(config, scopeFactory, logger)
    {
    }

    protected override async Task<bool> ProcessMessageAsync(string body, IServiceProvider serviceProvider)
    {
        var message = JsonSerializer.Deserialize<QueuePayload<JsonElement>>(body);

        // El worker de Telegram escucha su propia cola, pero el tipo de payload sigue siendo de notificación
        if (message == null) return false;

        var telegramData = JsonSerializer.Deserialize<WelcomeNotification>(message.Data.GetRawText());
        if (telegramData == null) return false;

        // Resolvemos el servicio de telegram desde el scope actual
        var telegramService = serviceProvider.GetRequiredService<ITelegramService>();
        
        _logger.LogInformation("[TelegramWorker] Procesando notificación para {Name}", telegramData.Name);
        return await telegramService.SendNotificationAsync(telegramData);
    }
}


