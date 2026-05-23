using DotNetKafkaRabbitMQExample.Application.Events;
using DotNetKafkaRabbitMQExample.Application.Interfaces;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(ILogger<TelegramService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendNotificationAsync(WelcomeNotification message)
    {
        try
        {
            // Simulación de envío a Telegram (usando HttpClient o una librería de Telegram)
            await Task.Delay(150);
            _logger.LogInformation("[TelegramService] Notificación enviada a Telegram para: {Name} ({Email})", message.Name, message.To);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TelegramService] Error enviando a Telegram");
            return false;
        }
    }
}


