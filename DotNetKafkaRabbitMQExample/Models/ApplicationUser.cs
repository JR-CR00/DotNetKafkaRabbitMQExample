using System;
using Microsoft.AspNetCore.Identity;

namespace DotNetKafkaRabbitMQExample.Models;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
}
