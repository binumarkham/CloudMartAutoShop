using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class RepairOrderPart
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    public int RepairOrderId { get; set; }

    public int? SupplierId { get; set; }

    [MaxLength(100)]
    public string? PartNumber { get; set; }

    [Required]
    [MaxLength(300)]
    public string Description { get; set; } = "";

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public int CreatedByUserId { get; set; }

    [MaxLength(150)]
    public string CreatedByName { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RepairOrder RepairOrder { get; set; } = null!;

    public Supplier? Supplier { get; set; }
}