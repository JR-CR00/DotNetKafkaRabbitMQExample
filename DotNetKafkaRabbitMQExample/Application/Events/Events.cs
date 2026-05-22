namespace Application.Events;

// Evento que Kafka transporta cuando un usuario se registra
public class UserRegisteredEvent
{
    public string UserId    { get; set; } = string.Empty;
    public string Username  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Name      { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

// Mensaje que RabbitMQ despacha al worker de email
public class SendWelcomeEmailMessage
{
    public string To      { get; set; } = string.Empty;
    public string Name    { get; set; } = string.Empty;
    public string UserId  { get; set; } = string.Empty;
}
