using CloudMartAutoShop.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CloudMartAutoShop.Api.Data;

public static class DbSeeder
{
    public static async Task SeedDevelopmentAdminAsync(
        AppDbContext db,
        IConfiguration configuration)
    {
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        var name = configuration["SeedAdmin:Name"] ?? "Administrator";
        var businessName =
            configuration["SeedAdmin:BusinessName"] ?? "Auto Shop Test";

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        email = email.Trim().ToLowerInvariant();

        var business = await db.Businesses
            .FirstOrDefaultAsync(x => x.Name == businessName);

        if (business is null)
        {
            business = new Business
            {
                Name = businessName,
                CreatedAt = DateTime.UtcNow
            };

            db.Businesses.Add(business);
            await db.SaveChangesAsync();
        }

        var existingUser = await db.Users
            .FirstOrDefaultAsync(x =>
                x.BusinessId == business.Id &&
                x.Email.ToLower() == email);

        if (existingUser is not null)
        {
            return;
        }

        var user = new User
        {
            BusinessId = business.Id,
            Email = email,
            Name = name,
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<User>();

        user.PasswordHash =
            passwordHasher.HashPassword(user, password);

        db.Users.Add(user);

        await db.SaveChangesAsync();
    }
}