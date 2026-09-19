using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class RepairOrderLabor
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    public int RepairOrderId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = "";

    [MaxLength(150)]
    public string? TechnicianName { get; set; }

    [MaxLength(2000)]
    public string? TechnicianNotes { get; set; }

    public decimal Hours { get; set; }

    public decimal HourlyRate { get; set; }

    public decimal LineTotal { get; set; }

    public int CreatedByUserId { get; set; }

    [MaxLength(150)]
    public string CreatedByName { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RepairOrder RepairOrder { get; set; } = null!;
}