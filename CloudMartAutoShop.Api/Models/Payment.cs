using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class Payment
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    public int RepairOrderId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "Other";

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public int CreatedByUserId { get; set; }

    [MaxLength(150)]
    public string CreatedByName { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RepairOrder RepairOrder { get; set; } = null!;
}