using DotNetKafkaRabbitMQExample.Infrastructure.Messaging.Kafka;
using DotNetKafkaRabbitMQExample.Infrastructure.Messaging.RabbitMQ;
using DotNetKafkaRabbitMQExample.Domain.Entities;
using Confluent.Kafka;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using DotNetKafkaRabbitMQExample.Application.Events;
using Confluent.Kafka.Admin;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Workers;

// BackgroundService corre en un hilo separado sin bloquear tu API
public class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly string[] KafkaTopic = KafkaTopics.All;
    private readonly string[] RMQQueues = RabbitMQQueue.All;
    private const string ConsumerGroup = "notification-service";

    public KafkaConsumerService(IConfiguration config)
    {
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Corre en un hilo del thread pool para no bloquear el startup
        await Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private async Task ConsumeLoop(CancellationToken stoppingToken)
    {
        // ─── Configuración del Consumer Kafka ──────────────────────
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = ConsumerGroup,

            // earliest = procesa desde el inicio si es la primera vez
            // latest   = procesa solo mensajes nuevos
            AutoOffsetReset = AutoOffsetReset.Earliest,

            // Desactivamos el auto-commit para hacer commit manual
            // Solo confirmamos el offset DESPUÉS de publicar en RabbitMQ
            EnableAutoCommit = false
        };


        await EnsureTopicsExistAsync();

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(KafkaTopic);

        // ─── Conexión a RabbitMQ ────────────────────────────────────
        var rabbitFactory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "localhost",
            UserName = _config["RabbitMQ:Username"] ?? "admin",
            Password = _config["RabbitMQ:Password"] ?? "admin123"
        };

        IConnection rabbitConn = await rabbitFactory.CreateConnectionAsync();
        IChannel rabbitChannel = await rabbitConn.CreateChannelAsync();

        await CreateRabbitMQQueuesAsync(rabbitChannel);

        Console.WriteLine($"[Consumer] Escuchando topics '{string.Join(", ", KafkaTopic)}'...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                if (consumeResult == null) continue;

                var evt = JsonSerializer.Deserialize<UserRegisteredEvent>(consumeResult.Message.Value);
                if (evt == null) continue;

                switch (consumeResult.Topic)
                {
                    case KafkaTopics.UserRegistered:
                        Console.WriteLine($"[Consumer] Evento recibido → UserId: {evt.UserId} | Email: {evt.Email}");
                        await HandleUserRegisteredAsync(evt, rabbitChannel);
                        break;

                    default:
                        Console.WriteLine($"[Consumer] Topic no manejado: {consumeResult.Topic}");
                        break;
                }

                Console.WriteLine($"[Consumer] Tarea de email publicada en RabbitMQ → {evt.Email}");

                consumer.Commit(consumeResult);
            }
            catch (OperationCanceledException)
            {
                break; // Shutdown limpio
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Consumer] Error: {ex.Message}");
            }
        }

        consumer.Close();
    }


    private async Task HandleUserRegisteredAsync(UserRegisteredEvent evt, IChannel rabbitChannel)
    {
        var notificationData = new WelcomeNotification
        {
            To = evt.Email,
            Name = evt.Name,
            UserId = evt.UserId
        };

        // ─── Publicar en la cola de Email ────────────────
        var emailPayload = new QueuePayload<WelcomeNotification>(QueueName.EmailQueue, notificationData);
        var emailBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(emailPayload));

        var props = new BasicProperties { Persistent = true };

        await rabbitChannel.BasicPublishAsync(
            exchange: "",
            routingKey: RabbitMQQueue.NotificationsEmail,
            mandatory: true,
            basicProperties: props,
            body: emailBody
        );

        // ─── Publicar en la cola de Telegram ────────────────
        var telegramPayload = new QueuePayload<WelcomeNotification>(QueueName.NotificationQueue, notificationData);
        var telegramBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(telegramPayload));

        await rabbitChannel.BasicPublishAsync(
            exchange: "",
            routingKey: RabbitMQQueue.NotificationsTelegram,
            mandatory: true,
            basicProperties: props,
            body: telegramBody
        );
    }


    private async Task EnsureTopicsExistAsync()
    {
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"]
        };

        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        try
        {
            var topics = KafkaTopics.All.Select(t => new TopicSpecification
            {
                Name = t,
                NumPartitions = 1,
                ReplicationFactor = 1
            }).ToArray();

            await adminClient.CreateTopicsAsync(topics);

            Console.WriteLine($"[Kafka] Topics creados: {string.Join(", ", KafkaTopics.All)}");
        }
        catch (CreateTopicsException ex)
        {
            foreach (var result in ex.Results)
            {
                if (result.Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    Console.WriteLine($"[Kafka] Topic '{result.Topic}' ya existe.");
                }
                else
                {
                    Console.WriteLine(
                        $"[Kafka] Error creando topic '{result.Topic}': {result.Error.Reason}"
                    );

                    throw;
                }
            }
        }
    }

    private async Task CreateRabbitMQQueuesAsync(IChannel channel)
    {
        foreach (var queue in RMQQueues)
        {
            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                    {
                        { "x-dead-letter-exchange", "" },
                        { "x-dead-letter-routing-key", $"{queue}.dlq" }
                    }
            );

            await channel.QueueDeclareAsync(
                queue: $"{queue}.dlq",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            Console.WriteLine($"[RabbitMQ] Cola '{queue}' asegurada.");
        }
    }
}






