using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class User
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "User";

    // Internal labor billing rate. Used only for Technician users.
    public decimal? HourlyRate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;
}