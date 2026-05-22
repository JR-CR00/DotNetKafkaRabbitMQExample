using System;
using DotNetKafkaRabbitMQExample.Models;
using DotNetKafkaRabbitMQExample.Models.Dto;

namespace DotNetKafkaRabbitMQExample.Repository.IRepository;

public interface IUserRepository
{
    ICollection<ApplicationUser> GetUsers();
    ApplicationUser? GetUser(string id);
    bool IsUniqueUser(string username);
    Task<UserLoginResponseDto> Login(UserLoginDto request);
    Task<UserDataDto> Register(CreateUserDto request);
}
