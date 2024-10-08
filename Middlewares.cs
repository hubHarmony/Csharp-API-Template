using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Simple_API;

public static class Middlewares
{
    public class ClaimsValidationMiddleware (RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.GivenName)?.Value;
                var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                {
                    Console.WriteLine(context.Connection.RemoteIpAddress?.ToString());
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Reported detected spoofed JWT in request header to admins.");
                    return;
                }
            }
            await next(context);
        }
    }
}

public static class LightCrypto
{
    private const string IdCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const int IdLength = 28;
    
    public static string GenerateRandomId()
    {
        byte[] randomBytes = new byte[IdLength];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        char[] idChars = new char[IdLength];

        for (int i = 0; i < IdLength; i++)
        {
            idChars[i] = IdCharacters[randomBytes[i] % IdCharacters.Length];
        }

        return new string(idChars);
    }
    
    public static string GenerateJwtToken(string email, string role, string userId, IConfiguration configuration)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.GivenName, userId),
        };

        var configKey = configuration["Jwt:Key"];

        if (string.IsNullOrEmpty(configKey))
        {
            return string.Empty;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(190),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public static string HashPassword(string password)
    {
        using var hmac = new HMACSHA256();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string hashedPassword, string password)
    {
        var parts = hashedPassword.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        using var hmac = new HMACSHA256(salt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(hash);
    }
}

public static class DatabaseUtils
{ 
    public static async Task<string> GenerateUniqueIdAsync(Database.Database context)
    {
        string uniqueId;

        do
        {
            uniqueId = LightCrypto.GenerateRandomId();
        } while (await IdExistsInDatabase(context, uniqueId));

        return uniqueId;
    }
        
    private static async Task<bool> IdExistsInDatabase(Database.Database context, string id)
    {
        return await context.Users.AnyAsync(user => user.Id == id);
    }
}