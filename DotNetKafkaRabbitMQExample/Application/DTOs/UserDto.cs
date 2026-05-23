using DotNetKafkaRabbitMQExample.Domain.Entities;
using System;

namespace DotNetKafkaRabbitMQExample.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
}




