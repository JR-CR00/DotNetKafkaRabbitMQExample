using System;

namespace DotNetKafkaRabbitMQExample.Models.Dto;

public class UserRegisterDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public required string? Username { get; set; }
    public required string? Password { get; set; }
    public string? Role { get; set; }
}
