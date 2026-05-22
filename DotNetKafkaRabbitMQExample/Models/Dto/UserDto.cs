using System;

namespace DotNetKafkaRabbitMQExample.Models.Dto;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
}
