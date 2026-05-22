using Mapster;
using DotNetKafkaRabbitMQExample.Models;
using DotNetKafkaRabbitMQExample.Models.Dto;

namespace DotNetKafkaRabbitMQExample.Mapping;

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
