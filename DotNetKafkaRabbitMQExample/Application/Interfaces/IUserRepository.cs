using System;
using DotNetKafkaRabbitMQExample.Domain.Entities;
using DotNetKafkaRabbitMQExample.Application.DTOs;

namespace DotNetKafkaRabbitMQExample.Application.Interfaces;

public interface IUserRepository
{
    ICollection<ApplicationUser> GetUsers();
    ApplicationUser? GetUser(string id);
    bool IsUniqueUser(string username);
    Task<UserLoginResponseDto> Login(UserLoginDto request);
    Task<UserDataDto> Register(CreateUserDto request);
}




