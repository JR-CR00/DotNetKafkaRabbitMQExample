namespace DotNetKafkaRabbitMQExample.Application.Events;


public enum QueueName
{
    EmailQueue,
    NotificationQueue,
}

// Evento que Kafka transporta cuando un usuario se registra
public class UserRegisteredEvent
{
    public string UserId    { get; set; } = string.Empty;
    public string Username  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Name      { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

// Mensaje neutral que RabbitMQ despacha a los distintos adaptadores
public class WelcomeNotification
{
    public string To      { get; set; } = string.Empty;
    public string Name    { get; set; } = string.Empty;
    public string UserId  { get; set; } = string.Empty;
}

public class OrderPlacedEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}





