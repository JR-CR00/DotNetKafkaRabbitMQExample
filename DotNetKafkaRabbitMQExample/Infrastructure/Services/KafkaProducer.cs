using DotNetKafkaRabbitMQExample.Infrastructure.Services;
using DotNetKafkaRabbitMQExample.Domain.Entities;
using Confluent.Kafka;
using System.Text.Json;
using DotNetKafkaRabbitMQExample.Application.Events;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Services;

public interface IKafkaProducer
{
    Task PublishUserRegisteredAsync(UserRegisteredEvent evt);
}

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private const string Topic = "user.registered";

    public KafkaProducer(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",

            // Garantiza que el mensaje llegó al broker antes de continuar
            Acks = Acks.All,

            // Reintenta hasta 3 veces si hay error transitorio
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 500
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishUserRegisteredAsync(UserRegisteredEvent evt)
    {
        var message = new Message<string, string>
        {
            // UserId como Key garantiza que eventos del mismo usuario
            // siempre vayan a la misma partición (orden garantizado)
            Key   = evt.UserId,
            Value = JsonSerializer.Serialize(evt)
        };

        var result = await _producer.ProduceAsync(Topic, message);

        Console.WriteLine($"[Kafka] Evento publicado → Topic: {result.Topic} | " +
                          $"Partition: {result.Partition} | Offset: {result.Offset}");
    }

    public void Dispose() => _producer?.Dispose();
}



