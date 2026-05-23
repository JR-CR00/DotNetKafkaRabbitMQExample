using DotNetKafkaRabbitMQExample.Infrastructure.Messaging.Kafka;
namespace DotNetKafkaRabbitMQExample.Infrastructure.Messaging.RabbitMQ;


public static class RabbitMQQueue
{
    public const string NotificationsEmail = "notifications.email";
    public const string NotificationsSms = "notifications.sms";
    public const string NotificationsTelegram = "notifications.telegram";

    public static readonly string[] All = [NotificationsEmail, NotificationsSms, NotificationsTelegram];
}





