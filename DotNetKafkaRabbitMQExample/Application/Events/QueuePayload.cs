namespace DotNetKafkaRabbitMQExample.Application.Events;

public class QueuePayload<T>
{
    public QueueName Type { get; set; }

    public T Data { get; set; }

    public QueuePayload(QueueName type, T data)
    {
        Type = type;
        Data = data;
    }
}


