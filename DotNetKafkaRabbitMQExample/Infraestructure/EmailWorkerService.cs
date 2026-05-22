using System.Text;
using System.Text.Json;
using Application.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Workers;

public class EmailWorkerService : BackgroundService
{
    private readonly IConfiguration _config;
    private const string EmailQueue = "notifications.email";
    private const int MaxRetries = 3;

    public EmailWorkerService(IConfiguration config)
    {
        _config = config;
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

        IConnection connection = await factory.CreateConnectionAsync();
        IChannel channel = await connection.CreateChannelAsync();


        // Prefetch = 1: el worker toma UN mensaje a la vez
        // No toma el siguiente hasta terminar el actual
        // Esto distribuye la carga equitativamente entre múltiples workers
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer =  new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<SendWelcomeEmailMessage>(body);

            if (message == null)
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                return;
            }

            var retryCount = GetRetryCount(ea);
            Console.WriteLine($"[EmailWorker] Procesando email → {message.To} (intento {retryCount + 1})");

            var success = await SendWelcomeEmailAsync(message);

            if (success)
            {
                // ACK: confirmamos que el email fue enviado
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                Console.WriteLine($"[EmailWorker] Email enviado exitosamente → {message.To}");
            }
            else if (retryCount < MaxRetries)
            {
                // NACK con requeue: RabbitMQ lo reencola para reintento
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
                Console.WriteLine($"[EmailWorker] Fallo, reintentando... ({retryCount + 1}/{MaxRetries})");
            }
            else
            {
                // Superó los reintentos → va a la Dead Letter Queue
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                Console.WriteLine($"[EmailWorker] Max reintentos superados → DLQ: {message.To}");
            }
        };

        await channel.BasicConsumeAsync(queue: EmailQueue, autoAck: false, consumer: consumer);

        Console.WriteLine("[EmailWorker] Esperando mensajes de email...");

        // Mantiene el worker activo hasta que se cancele
        while (!stoppingToken.IsCancellationRequested)
            Thread.Sleep(100);
    }

    private async Task<bool> SendWelcomeEmailAsync(SendWelcomeEmailMessage message)
    {
        try
        {
            // Aquí integras tu servicio de email real:
            // SendGrid, SMTP, AWS SES, Mailgun, etc.
            //
            // Ejemplo con SendGrid:
            // await _sendGridClient.SendEmailAsync(new SendGridMessage
            // {
            //     Subject = "¡Bienvenido!",
            //     HtmlContent = $"<h1>Hola {message.Name}, bienvenido a la plataforma.</h1>"
            // });

            // Por ahora simulamos el envío
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

    private static int GetRetryCount(BasicDeliverEventArgs ea)
    {
        if (ea.BasicProperties.Headers != null &&
            ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryObj) &&
            retryObj is int retryCount)
            return retryCount;
        return 0;
    }
}
