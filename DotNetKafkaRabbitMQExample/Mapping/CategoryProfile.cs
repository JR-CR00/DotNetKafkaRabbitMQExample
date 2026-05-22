using Mapster;
using DotNetKafkaRabbitMQExample.Models;
using DotNetKafkaRabbitMQExample.Models.Dto;

namespace DotNetKafkaRabbitMQExample.Mapping;

public class CategoryProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Category, CategoryDto>().TwoWays();
        config.NewConfig<Category, CreateCategoryDto>().TwoWays();
    }
}
