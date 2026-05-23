using DotNetKafkaRabbitMQExample.Domain.Entities;
using System;

namespace DotNetKafkaRabbitMQExample.Application.DTOs;

public class UserDataDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
}


