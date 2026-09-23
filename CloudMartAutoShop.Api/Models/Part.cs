using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class Part
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    [MaxLength(100)]
    public string? PartNumber { get; set; }

    [Required]
    [MaxLength(300)]
    public string Description { get; set; } = "";

    public int? PreferredSupplierId { get; set; }

    public decimal DefaultUnitCost { get; set; }

    public decimal DefaultUnitPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Business Business { get; set; } = null!;

    public Supplier? PreferredSupplier { get; set; }

    public ICollection<PartSupplier> Suppliers { get; set; }
        = new List<PartSupplier>();
}