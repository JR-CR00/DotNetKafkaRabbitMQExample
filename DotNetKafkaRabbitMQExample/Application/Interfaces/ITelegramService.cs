using DotNetKafkaRabbitMQExample.Application.Events;

namespace DotNetKafkaRabbitMQExample.Application.Interfaces;

public interface ITelegramService
{
    Task<bool> SendNotificationAsync(WelcomeNotification message);
}


