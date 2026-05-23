using Mapster;
using DotNetKafkaRabbitMQExample.Domain.Entities;
using DotNetKafkaRabbitMQExample.Application.DTOs;

namespace DotNetKafkaRabbitMQExample.Application.Mappings;

public class ProductProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .TwoWays();

        config.NewConfig<Product, CreateProductDto>().TwoWays();
        config.NewConfig<Product, UpdateProductDto>().TwoWays();
    }
}




