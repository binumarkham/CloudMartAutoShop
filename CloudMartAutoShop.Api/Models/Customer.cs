using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class Customer
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = "";

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? ProvinceState { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}