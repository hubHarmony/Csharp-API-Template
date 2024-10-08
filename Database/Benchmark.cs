using System.Diagnostics;
using Faker;
using Microsoft.EntityFrameworkCore;

namespace Simple_API.Database;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Benchmark (Database context)
{
    
    private readonly HashSet<string> _usedFirstNames = new HashSet<string>();
    private readonly HashSet<string> _usedLastNames = new HashSet<string>();

    public async Task<TimeSpan> GenerateRandomUsersAsync(int numberOfUsers)
    {
        var random = new Random();
        var users = new List<User>();

        for (int i = 0; i < numberOfUsers; i++)
        {
            var firstName = GenerateRandomFirstName();
            var lastName = GenerateRandomLastName();
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}@hub-harmony.fr";
            var phoneNumber = $"+33 6 {GenerateRandomPhoneNumber()}";
            var birthday = GenerateRandomBirthday(random);
            var password = GenerateRandomPassword();

            var uniqueId = await DatabaseUtils.GenerateUniqueIdAsync(context);

            var user = new User
            {
                Id = uniqueId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = LightCrypto.HashPassword(password),
                Birthday = birthday,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsAdmin = false
            };

            users.Add(user);
        }

        var watch = Stopwatch.StartNew();
        TimeSpan time;

        try
        {
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new Exception($"Error: Could not create users. {e.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error: Could not create users. {ex.Message}");
        }
        finally
        {
            watch.Stop();
            time = watch.Elapsed;
        }
        return time;
    }
    
    private string GenerateRandomFirstName()
    {
        string name;
        do
        {
            name = Name.First();
        } while (_usedFirstNames.Contains(name));

        _usedFirstNames.Add(name);
        return name;
    }

    private string GenerateRandomLastName()
    {
        string name;
        do
        {
            name = Name.Last();
        } while (_usedLastNames.Contains(name));

        _usedLastNames.Add(name);
        return name;
    }

    private string GenerateRandomPhoneNumber()
    {
        Random random = new Random();
        return string.Join("", Enumerable.Range(0, 8).Select(_ => random.Next(0, 10).ToString()));
    }

    private DateTime GenerateRandomBirthday(Random random)
    {
        int year = random.Next(DateTime.Now.Year - 80, DateTime.Now.Year);
        int month = random.Next(1, 13);
        int day;

        if (month == 2)
        {
            day = random.Next(1, 29);
        }
        else if (month == 4 || month == 6 || month == 9 || month == 11)
        {
            day = random.Next(1, 31);
        }
        else
        {
            day = random.Next(1, 32);
        }

        return new DateTime(year, month, day);
    }

    private string GenerateRandomPassword()
    {
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string specialChars = "@$!%*?&";

        Random random = new Random();
        
        char[] passwordChars = new char[12];
        
        passwordChars[0] = lowerChars[random.Next(lowerChars.Length)];
        passwordChars[1] = upperChars[random.Next(upperChars.Length)];
        passwordChars[2] = digits[random.Next(digits.Length)];
        passwordChars[3] = specialChars[random.Next(specialChars.Length)];

        for (int i = 4; i < passwordChars.Length; i++)
        {
            string allChars = lowerChars + upperChars + digits + specialChars;
            passwordChars[i] = allChars[random.Next(allChars.Length)];
        }

        return new string(passwordChars.OrderBy(x => random.Next()).ToArray());
    }
}