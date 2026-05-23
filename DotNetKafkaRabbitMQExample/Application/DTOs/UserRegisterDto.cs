using DotNetKafkaRabbitMQExample.Domain.Entities;
using System;

namespace DotNetKafkaRabbitMQExample.Application.DTOs;

public class UserRegisterDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public required string? Username { get; set; }
    public required string? Password { get; set; }
    public string? Role { get; set; }
}


