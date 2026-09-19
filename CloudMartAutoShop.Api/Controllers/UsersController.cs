using System.ComponentModel.DataAnnotations;
using CloudMartAutoShop.Api.Data;
using CloudMartAutoShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudMartAutoShop.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Administrator")]
[Route("api/[controller]")]
public class UsersController(AppDbContext db) : TenantControllerBase
{
    private static readonly string[] AllowedRoles = ["Administrator", "Service Advisor", "Technician"];

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await db.Users.AsNoTracking()
        .Where(x => x.BusinessId == BusinessId)
        .OrderBy(x => x.Name)
        .Select(x => new UserAdminDto(x.Id, x.Name, x.Email, x.Role, x.IsActive, x.CreatedAt))
        .ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateRequest request)
    {
        var error = Validate(request.Name, request.Email, request.Role, request.Password, true);
        if (error is not null) return BadRequest(error);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.BusinessId == BusinessId && x.Email.ToLower() == email))
            return Conflict("A user with this email already exists for this business.");
        var user = new User { BusinessId = BusinessId, Name = request.Name.Trim(), Email = email, Role = NormalizeRole(request.Role), IsActive = request.IsActive, CreatedAt = DateTime.UtcNow };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password!);
        db.Users.Add(user); await db.SaveChangesAsync();
        return Ok(new UserAdminDto(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UserUpdateRequest request)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id && x.BusinessId == BusinessId);
        if (user is null) return NotFound();
        var error = Validate(request.Name, request.Email, request.Role, request.Password, false);
        if (error is not null) return BadRequest(error);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.BusinessId == BusinessId && x.Id != id && x.Email.ToLower() == email))
            return Conflict("A user with this email already exists for this business.");
        if (id == UserId && !request.IsActive) return BadRequest("You cannot deactivate your own account.");
        if (id == UserId && NormalizeRole(request.Role) != "Administrator") return BadRequest("You cannot remove your own Administrator role.");
        user.Name = request.Name.Trim(); user.Email = email; user.Role = NormalizeRole(request.Role); user.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        await db.SaveChangesAsync(); return NoContent();
    }

    private static string? Validate(string name, string email, string role, string? password, bool passwordRequired)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required.";
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email)) return "A valid email is required.";
        if (!AllowedRoles.Contains(NormalizeRole(role))) return "Role must be Administrator, Service Advisor, or Technician.";
        if (passwordRequired && string.IsNullOrWhiteSpace(password)) return "Password is required.";
        if (!string.IsNullOrEmpty(password) && password.Length < 8) return "Password must be at least 8 characters.";
        return null;
    }
    private static string NormalizeRole(string role) => role.Trim() switch { "Admin" => "Administrator", "Administrator" => "Administrator", "Service Advisor" => "Service Advisor", "Technician" => "Technician", _ => role.Trim() };
}

public record UserAdminDto(int Id, string Name, string Email, string Role, bool IsActive, DateTime CreatedAt);
public class UserCreateRequest { public string Name { get; set; }=""; public string Email { get; set; }=""; public string Role { get; set; }="Technician"; public string? Password { get; set; } public bool IsActive { get; set; }=true; }
public class UserUpdateRequest : UserCreateRequest { }
