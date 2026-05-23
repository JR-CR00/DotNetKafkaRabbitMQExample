using DotNetKafkaRabbitMQExample.Domain.Entities;
using System;

namespace DotNetKafkaRabbitMQExample.Application.DTOs;

public class UserLoginResponseDto
{
    public UserDataDto? User { get; set; }
    public string ? Token { get; set; }
    public string? Message { get; set; }
}


