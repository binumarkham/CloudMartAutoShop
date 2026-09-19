using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CloudMartAutoShop.Api.Data;
using CloudMartAutoShop.Api.DTOs;
using CloudMartAutoShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CloudMartAutoShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AppDbContext db,
    IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .Include(x => x.Business)
            .SingleOrDefaultAsync(x =>
                x.Email.ToLower() == email);

        if (user is null || !user.IsActive)
        {
            return Unauthorized("Invalid email or password.");
        }

        var passwordHasher = new PasswordHasher<User>();

        var passwordResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key was not found.");

        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer was not found.");

        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience was not found.");

        var expiresMinutes =
            int.TryParse(
                configuration["Jwt:ExpiresMinutes"],
                out var minutes)
                ? minutes
                : 60;

        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Name,
                user.Name),

            new(
                ClaimTypes.Role,
                user.Role),

            new(
                "business_id",
                user.BusinessId.ToString()),

            new(
                "business_name",
                user.Business.Name),

            new(
                "user_name",
                user.Name)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenText =
            new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse
        {
            Token = tokenText,
            BusinessName = user.Business.Name,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            ExpiresAt = expiresAt
        });
    }
}