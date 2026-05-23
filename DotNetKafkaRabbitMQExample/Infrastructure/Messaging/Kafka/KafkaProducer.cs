using Confluent.Kafka;
using System.Text.Json;
using DotNetKafkaRabbitMQExample.Application.Events;
using DotNetKafkaRabbitMQExample.Infrastructure.Messaging.RabbitMQ;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Messaging.Kafka;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, string key, T value);
    Task PublishUserRegisteredAsync(UserRegisteredEvent evt);
}

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration config, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 500
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishAsync<T>(string topic, string key, T value)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = JsonSerializer.Serialize(value)
            };

            var result = await _producer.ProduceAsync(topic, message);

            _logger.LogInformation("[Kafka] Mensaje publicado -> Topic: {Topic} | Partition: {Partition} | Offset: {Offset}", 
                result.Topic, result.Partition, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Error publicando en el topic {Topic}", topic);
            throw;
        }
    }

    public async Task PublishUserRegisteredAsync(UserRegisteredEvent evt)
    {
        await PublishAsync(KafkaTopics.UserRegistered, evt.UserId, evt);
    }

    public void Dispose() => _producer?.Dispose();
}


