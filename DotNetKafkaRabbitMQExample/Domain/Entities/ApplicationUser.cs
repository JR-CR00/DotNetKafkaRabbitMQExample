using System;
using Microsoft.AspNetCore.Identity;

namespace DotNetKafkaRabbitMQExample.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
}




