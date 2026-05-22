using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DotNetKafkaRabbitMQExample.Models;
using DotNetKafkaRabbitMQExample.Models.Dto;
using DotNetKafkaRabbitMQExample.Repository.IRepository;

namespace DotNetKafkaRabbitMQExample.Repository;

public class UserRepository : IUserRepository
{

    public readonly ApplicationDbContext _db;
    private readonly string? _secretKey;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserRepository(ApplicationDbContext db, IConfiguration configuration,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager, IMapper mapper)
    {
        _secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public ApplicationUser? GetUser(string id)
    {
        return _db.ApplicationUsers.FirstOrDefault(u => u.Id.Trim().ToLower() == id.Trim().ToLower());
    }

    public ICollection<ApplicationUser> GetUsers()
    {
        return _db.ApplicationUsers.OrderBy(x => x.UserName).ToList();
    }

    public bool IsUniqueUser(string username)
    {
        return !_db.ApplicationUsers.Any(u => u.UserName != null && u.UserName.ToLower().Trim() == username.Trim().ToLower());
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return new UserLoginResponseDto()
            {
                Message = "Username and password are required.",
                Token = string.Empty,
                User = null
            };
        }

        var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(u => u.UserName != null && u.UserName.ToLower().Trim() == request.Username.ToLower().Trim());

        if (user is null)
        {
            return new UserLoginResponseDto()
            {
                Message = "Invalid username or password.",
                Token = string.Empty,
                User = null
            };
        }

        bool isValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isValid)
        {
            return new UserLoginResponseDto()
            {
                Message = "Invalid username or password.",
                Token = string.Empty,
                User = null
            };
        }

        // Aquí se generaría el token JWT utilizando _secretKey y la información del usuario
        var handlerToken = new JwtSecurityTokenHandler();
        if (_secretKey is null)
        {
            throw new InvalidOperationException("Secret key is not configured.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("Username", user.UserName ?? string.Empty),
                new Claim("Role", roles.FirstOrDefault() ?? string.Empty)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handlerToken.CreateToken(tokenDescriptor);

        return new UserLoginResponseDto()
        {
            Message = "Login successful.",
            Token = handlerToken.WriteToken(token),
            User = _mapper.Map<UserDataDto>(user)
        };
    }

    public async Task<UserDataDto> Register(CreateUserDto request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException("Username and password are required.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Name = request.Name,
            Email = request.Username, // Assuming email is the same as username, adjust if needed
            NormalizedUserName = request.Username.ToUpper()
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new ApplicationException("Error occurred while registering the user.");
        }

        var userRole = request.Role ?? "User"; // Default role is "User" if not provided
        var roleExists = await _roleManager.RoleExistsAsync(userRole);

        if (!roleExists)
        {
            var roleIdentity = new IdentityRole(userRole);
            await _roleManager.CreateAsync(roleIdentity);
        }

        await _userManager.AddToRoleAsync(user, userRole);

        var createdUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == request.Username);
        if (createdUser == null)
        {
            throw new InvalidOperationException("User registration failed: User not found after creation.");
        }

        return _mapper.Map<UserDataDto>(createdUser);

    }
}
