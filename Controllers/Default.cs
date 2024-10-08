using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simple_API.Database;

namespace Simple_API.Controllers
{

    [Route("Auth/")]
    [ApiController]
    public class Default(IConfiguration configuration, UserService userService) : ControllerBase
    {
        public static class UserRoles
        {
            public const string User = "User";
            public const string Admin = "Admin";
        }
        
        public class LoginAuthPayload
        {
            [DataType(DataType.EmailAddress)]
            [StringLength(100, ErrorMessage = "The email must be at max 100 characters long.")]
            [EmailAddress(ErrorMessage = "Invalid Email Address.")]
            [Required(ErrorMessage = "Email address is required.")]
            public string Email { get; init; } = string.Empty;


            [DataType(DataType.Password)]
            [Required(ErrorMessage = "Password is required.")]
            [StringLength(255, ErrorMessage = "Password must be between 8 and 255 characters.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter,"
                               + " one lowercase letter, one number, and one special character.")]
            public string Password { get; init; } = string.Empty;
        }
        
        public class RegisterAuthPayload : LoginAuthPayload
        {
            [Required(ErrorMessage = "First name is required.")]
            [RegularExpression(@"^[A-Z][a-zA-Z]*$", ErrorMessage =
                "First name must start with a capital letter and contain only letters.")]
            [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
            public string? FirstName { get; init; } = string.Empty;

            [Required(ErrorMessage = "Last name is required.")]
            [RegularExpression(@"^[A-Z][a-zA-Z]*$", ErrorMessage =
                "Last name must start with a capital letter and contain only letters.")]
            [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
            public string? LastName { get; init; } = string.Empty;

            [Required(ErrorMessage = "Birthday is required.")]
            [DataType(DataType.Date)]
            public DateTime? Birthday { get; init; }

            [Required(ErrorMessage = "Phone number is required.")]
            [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
            [RegularExpression(@"^(\+33|0)[1-9](\d{2}){4}$", ErrorMessage =
                "Phone number must be a valid French phone number (e.g., +33 6 12 34 56 78 or 06 12 34 56 78).")]
            public string? PhoneNumber { get; init; } = string.Empty;
        }
        

        [HttpPut("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterAuthPayload registerAuthPayload)
        {
            var existingUser = userService.GetUserByEmail(registerAuthPayload.Email);
            if (existingUser != null)
                return BadRequest("User already exists.");
            try
            {
                await userService.CreateUserAsync(registerAuthPayload);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
            return Ok("User created.");
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginAuthPayload loginAuthPayload)
        {
            var email = loginAuthPayload.Email;
            var user = userService.GetUserByEmail(email);
            if (user == null || !LightCrypto.VerifyPassword(user.Password, loginAuthPayload.Password))
            {
                return Unauthorized();
            }
            var token =
                LightCrypto.GenerateJwtToken(email: email,
                    role: user.IsAdmin ? UserRoles.Admin : UserRoles.User, userId: user.Id, configuration);
            return Ok(new { token = token });
        }
        
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }

        
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("benchmark/{numberOfUsers}")]
        public async Task<IActionResult> GenerateUsers(int numberOfUsers)
        {
            var time = await userService.BenchmarkUsers(numberOfUsers);
            
            return Ok($"Successfully created {numberOfUsers} user; database insertion took {time}.");
        }
        
    }

    [Route("[controller]")]
    [ApiController]
    public class Test : ControllerBase
    {
        public class TestPayload
        {
            [Required(ErrorMessage = "Data field is required.")]
            public string? Data { get; init; } = string.Empty;
        }
        
        private const string ProtocolOk = "Protocol tested successfully.";

        // GET: test/get
        [HttpGet("Get")]
        public IActionResult TestGet()
        {
            return Ok($"GET: {ProtocolOk}");
        }

        // POST: test/post
        [HttpPost("Post")]
        public IActionResult TestPost([FromBody] TestPayload testPayload)
        {
            return Ok($"POST: {ProtocolOk} Received: {testPayload.Data}");
        }

        // PUT: test/put
        [HttpPut("Put")]
        public IActionResult TestPut([FromBody] TestPayload testPayload)
        {
            return Ok($"PUT: {ProtocolOk} Updated: {testPayload.Data}");
        }

        // DELETE: test/delete
        [HttpDelete("Delete")]
        public IActionResult TestDelete([FromBody] TestPayload testPayload)
        {
            return Ok($"DELETE: {ProtocolOk} Deleted: {testPayload.Data}");
        }
    }

    [Route("Test/Protected")]
    [ApiController]
    public class ProtectedTest : ControllerBase
    {
        // Authorized GET: Test/Protected/Basic
        [Authorize]
        [HttpGet("Basic")]
        public IActionResult Basic()
        {
            var userId = User.FindFirst(ClaimTypes.GivenName)!.Value;
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            return Ok($"Successfully executed secured request. (Any user) as {role} with id {userId}");
        }
        
        // Authorized GET: Test/Protected/UserOnly
        [Authorize(Roles = Default.UserRoles.User)]
        [HttpGet("UserOnly")]
        public IActionResult UserOnly()
        {
            return Ok("Successfully executed secured request. (Users only)");
        }
        
        // Authorized GET: Test/Protected/AdminOnly
        [Authorize(Roles = Default.UserRoles.Admin)]
        [HttpGet("AdminOnly")]
        public IActionResult AdminOnly()
        {
            return Ok("Successfully executed secured request. (Admins only)");
        }
    }
}