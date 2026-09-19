using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class RepairOrder
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RepairOrderNumber { get; set; } = "";

    [MaxLength(50)]
    public string? InvoiceNumber { get; set; }

    public DateTime OpenedDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }

    public int? MileageIn { get; set; }
    public int? MileageOut { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Open";

    [MaxLength(2000)]
    public string? CustomerConcern { get; set; }

    [MaxLength(2000)]
    public string? Diagnosis { get; set; }

    [MaxLength(2000)]
    public string? WorkPerformed { get; set; }

    [MaxLength(150)]
    public string? TechnicianName { get; set; }

    // User assigned to perform the work. Kept nullable so existing repair orders remain valid.
    public int? AssignedTechnicianUserId { get; set; }

    [MaxLength(150)]
    public string? AssignedTechnicianName { get; set; }

    public decimal LaborSubtotal { get; set; }
    public decimal PartsSubtotal { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int CreatedByUserId { get; set; }

    [MaxLength(150)]
    public string CreatedByName { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Business Business { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;

    public ICollection<RepairOrderLabor> LaborLines { get; set; }
        = new List<RepairOrderLabor>();

    public ICollection<RepairOrderPart> PartLines { get; set; }
        = new List<RepairOrderPart>();

    public ICollection<Payment> Payments { get; set; }
        = new List<Payment>();
}