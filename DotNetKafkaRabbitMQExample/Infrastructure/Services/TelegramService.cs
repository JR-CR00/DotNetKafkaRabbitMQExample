using DotNetKafkaRabbitMQExample.Application.Events;
using DotNetKafkaRabbitMQExample.Application.Interfaces;
using Telegram.Bot;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly IConfiguration _config;
    private readonly TelegramBotClient _telegramClient;
    private readonly string _chatId;

    public TelegramService(ILogger<TelegramService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _telegramClient = new TelegramBotClient(_config["Telegram:BotToken"] ?? throw new ArgumentNullException("Telegram:BotToken"));
        _chatId = _config["Telegram:ChatId"] ?? throw new ArgumentNullException("Telegram:ChatId");
    }

    public async Task<bool> SendNotificationAsync(WelcomeNotification message)
    {
        try
        {
            await _telegramClient.SendMessage(
                chatId: _chatId,
                text: $"¡Hola {message.Name}! Bienvenido a nuestro servicio. \n Tu email es {message.To}."
            );
            // Simulación de envío a Telegram (usando HttpClient o una librería de Telegram)
           // await Task.Delay(150);
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


