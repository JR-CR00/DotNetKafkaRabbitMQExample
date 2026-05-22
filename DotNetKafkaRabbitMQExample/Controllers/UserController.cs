using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DotNetKafkaRabbitMQExample.Models.Dto;
using DotNetKafkaRabbitMQExample.Repository.IRepository;

namespace DotNetKafkaRabbitMQExample.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            var userDtos = _mapper.Map<IEnumerable<UserRegisterDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EnableCors("AllowSpecificOrigin")]
        public IActionResult GetUser(string id)
        {
            var user = _userRepository.GetUser(id);
            if (user == null)
                return NotFound($"User with id {id} not found.");

            var userDto = _mapper.Map<UserRegisterDto>(user);
            return Ok(userDto);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null)
                return BadRequest(ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_userRepository.IsUniqueUser(createUserDto.Username))
            {
                ModelState.AddModelError("CustomError", "Username already exists.");
                return BadRequest(ModelState);
            }

            var user = await _userRepository.Register(createUserDto);
            if (user == null)
            {
                ModelState.AddModelError("CustomError", "Error while registering user.");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (loginDto == null)
                return BadRequest(ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _userRepository.Login(loginDto);
            if (response == null || string.IsNullOrEmpty(response.Token))
            {
                ModelState.AddModelError("CustomError", response?.Message ?? "Invalid username or password.");
                return BadRequest(ModelState);
            }

            return Ok(response);
        }
    }
}
