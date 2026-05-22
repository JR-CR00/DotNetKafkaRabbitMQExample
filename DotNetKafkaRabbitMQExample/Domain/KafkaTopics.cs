

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