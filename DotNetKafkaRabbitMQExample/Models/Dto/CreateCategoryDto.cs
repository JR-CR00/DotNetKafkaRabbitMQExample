using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetKafkaRabbitMQExample.Models.Dto;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
    public string Name { get; set; } = string.Empty;
}
