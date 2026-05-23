using Mapster;
using DotNetKafkaRabbitMQExample.Domain.Entities;
using DotNetKafkaRabbitMQExample.Application.DTOs;

namespace DotNetKafkaRabbitMQExample.Application.Mappings;

public class UserProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>().TwoWays();
        config.NewConfig<User, UserRegisterDto>().TwoWays();
        config.NewConfig<User, UserLoginResponseDto>().TwoWays();
        config.NewConfig<User, CreateUserDto>().TwoWays();
        config.NewConfig<ApplicationUser, UserDataDto>().TwoWays();
        config.NewConfig<ApplicationUser, UserDto>().TwoWays();
    }
}


