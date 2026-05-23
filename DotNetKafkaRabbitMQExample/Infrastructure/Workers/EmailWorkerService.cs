using System.Text.Json;
using DotNetKafkaRabbitMQExample.Application.Events;
using DotNetKafkaRabbitMQExample.Application.Interfaces;
using DotNetKafkaRabbitMQExample.Infrastructure.Messaging.Kafka;
using DotNetKafkaRabbitMQExample.Infrastructure.Messaging.RabbitMQ;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Workers;

public class EmailWorkerService : RabbitMQConsumerBase
{
    protected override string TargetQueue => RabbitMQQueue.NotificationsEmail;

    public EmailWorkerService(
        IConfiguration config, 
        IServiceScopeFactory scopeFactory, 
        ILogger<EmailWorkerService> logger) 
        : base(config, scopeFactory, logger)
    {
    }

    protected override async Task<bool> ProcessMessageAsync(string body, IServiceProvider serviceProvider)
    {
        var message = JsonSerializer.Deserialize<QueuePayload<JsonElement>>(body);

        if (message == null || message.Type != QueueName.EmailQueue)
        {
            _logger.LogWarning("Mensaje inválido recibido en EmailWorker");
            return false;
        }

        var emailData = JsonSerializer.Deserialize<WelcomeNotification>(message.Data.GetRawText());
        if (emailData == null) return false;

        // Resolvemos el servicio de email desde el scope actual
        var emailService = serviceProvider.GetRequiredService<IEmailService>();
        
        _logger.LogInformation("Enviando email a {To}", emailData.To);
        return await emailService.SendWelcomeEmailAsync(emailData);
    }
}


