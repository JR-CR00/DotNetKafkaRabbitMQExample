using DotNetKafkaRabbitMQExample.Application.Events;

namespace DotNetKafkaRabbitMQExample.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendWelcomeEmailAsync(WelcomeNotification message);
}


