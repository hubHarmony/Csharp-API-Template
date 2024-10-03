using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace Simple_API.Controllers
{

    [Route("Auth/")]
    [ApiController]
    public class Default(IConfiguration configuration) : ControllerBase
    {
        public static class UserRoles
        {
            public const string User = "User";
            public const string Admin = "Admin";
        }
        
        public class AuthPayload
        {
            [DataType(DataType.EmailAddress)]
            [EmailAddress(ErrorMessage = "Invalid Email Address.")]
            [Required(ErrorMessage = "Email address is required.")]
            public string? Email { get; init; } = string.Empty;


            [DataType(DataType.Password)]
            [Required(ErrorMessage = "Password is required.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter,"
                               + " one lowercase letter, one number, and one special character.")]
            public string? Password { get; init; } = string.Empty;
        }

        [HttpPut("Register")]
        public IActionResult Register([FromBody] AuthPayload authPayload)
        {
            return Ok();
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] AuthPayload authPayload)
        {
            // Here, you would typically validate the user's credentials against a database.
            if (authPayload.Email == "test@example.com" && authPayload.Password == "Password123!")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, authPayload.Email),
                    new Claim(ClaimTypes.Role, UserRoles.User),
                    new Claim(ClaimTypes.GivenName, "Test_ID"),
                };

                var configKey = configuration["Jwt:Key"];

                if (string.IsNullOrEmpty(configKey))
                {
                    return StatusCode(500);
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(190),
                    signingCredentials: credentials);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized();
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
        [Authorize]
        [HttpGet("Basic")]
        public IActionResult Basic()
        {
            return Ok("Successfully executed secured request. (Any user)");
        }
        
        [Authorize(Roles = Default.UserRoles.User)]
        [HttpGet("UserOnly")]
        public IActionResult UserOnly()
        {
            return Ok("Successfully executed secured request. (User)");
        }
        
        [Authorize(Roles = Default.UserRoles.Admin)]
        [HttpGet("AdminOnly")]
        public IActionResult AdminOnly()
        {
            return Ok("Successfully executed secured request. (Admin)");
        }
    }
}