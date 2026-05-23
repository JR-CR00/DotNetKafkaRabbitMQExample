using DotNetKafkaRabbitMQExample.Application.Events;
using DotNetKafkaRabbitMQExample.Application.Interfaces;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task<bool> SendWelcomeEmailAsync(WelcomeNotification message)
    {
        try
        {
            // Simulación de envío de email
            await Task.Delay(100);
            Console.WriteLine($"[EmailWorker] Simulando envío a: {message.To} | Nombre: {message.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailWorker] Error enviando email: {ex.Message}");
            return false;
        }
    }
}


