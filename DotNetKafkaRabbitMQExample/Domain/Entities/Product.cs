using DotNetKafkaRabbitMQExample.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetKafkaRabbitMQExample.Domain.Entities;

public class Product
{

    public int Id { get; set; }
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    public string ImgUrl { get; set; } = string.Empty;
    public string? ImgUrlLocal { get; set; }

    [Required(ErrorMessage = "SKU is required.")]
    public string SKU { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; } = null;

    //foreign key
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public required Category Category { get; set; }

}


