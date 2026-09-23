using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class PartSupplier
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public int PartId { get; set; }

    public int SupplierId { get; set; }

    [MaxLength(100)]
    public string? SupplierPartNumber { get; set; }

    public decimal? LastCost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Part Part { get; set; } = null!;

    public Supplier Supplier { get; set; } = null!;
}