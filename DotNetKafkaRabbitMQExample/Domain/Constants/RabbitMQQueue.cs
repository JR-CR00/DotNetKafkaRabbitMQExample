using DotNetKafkaRabbitMQExample.Domain.Constants;
namespace DotNetKafkaRabbitMQExample.Domain.Constants;


public static class RabbitMQQueue
{
    public const string NotificationsEmail = "notifications.email";

    public static readonly string[] All =  [ NotificationsEmail  ];
}



