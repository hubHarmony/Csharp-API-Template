using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Simple_API.Controllers;

namespace Simple_API.Database
{
    
    public class Database(DbContextOptions<Database> options) : DbContext(options)
    {
        public DbSet<User> Users { get; init; }
    }


    [Table("TemplateUsers")]
    public class User : Default.RegisterAuthPayload
    {
        [Key] [StringLength(28)] public required string Id { get; init; } = Guid.NewGuid().ToString();

        [DataType(DataType.Date)] public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public bool IsAdmin { get; init; }
    }


    public class UserService(Database context)
    {
        public async Task<string> CreateUserAsync(Default.RegisterAuthPayload registerAuthPayload)
        {
            var uniqueId = await DatabaseUtils.GenerateUniqueIdAsync(context);

            var user = new User
            {
                Id = uniqueId,
                FirstName = registerAuthPayload.FirstName,
                LastName = registerAuthPayload.LastName,
                Email = registerAuthPayload.Email,
                Password = LightCrypto.HashPassword(registerAuthPayload.Password),
                Birthday = registerAuthPayload.Birthday,
                PhoneNumber = registerAuthPayload.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsAdmin = false
            };
            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new Exception($"Error : User already exists");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error : Could not create user");
            }
            return $"Successfully created user {registerAuthPayload.Email}";
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            try
            {
                return await context.Users.FindAsync(id);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Error fetching user: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}");
            }
        }
        
        public User? GetUserByEmail(string email)
        {
            return context.Users.FirstOrDefault(u => u.Email == email);
        }

        public async Task<TimeSpan> BenchmarkUsers(int numberOfUsers)
        {
            return await new Benchmark(context).GenerateRandomUsersAsync(numberOfUsers);
        }
    }
}