using DotNetKafkaRabbitMQExample.Domain.Entities;
using System;

namespace DotNetKafkaRabbitMQExample.Application.DTOs;

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImgUrl { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int CategoryId { get; set; }
}




