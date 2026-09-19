using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class Vehicle
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public int CustomerId { get; set; }

    [MaxLength(17)]
    public string? Vin { get; set; }

    public int? Year { get; set; }

    [MaxLength(100)]
    public string? Make { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(100)]
    public string? Trim { get; set; }

    [MaxLength(30)]
    public string? LicensePlate { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    public int? CurrentMileage { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;

    public Customer Customer { get; set; } = null!;

    public ICollection<RepairOrder> RepairOrders { get; set; }
        = new List<RepairOrder>();
}