using DotNetKafkaRabbitMQExample.Domain.Constants;
using DotNetKafkaRabbitMQExample.Domain.Entities;
namespace DotNetKafkaRabbitMQExample.Domain.Constants;



public static class KafkaTopics
{
    public const string UserRegistered = "user.registered";
    public const string OrderPlaced = "order.placed";

    public static readonly string[] All =
    [
        UserRegistered
        , OrderPlaced
    ];
}



