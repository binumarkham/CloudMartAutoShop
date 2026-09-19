
using System.ComponentModel.DataAnnotations;
namespace CloudMartAutoShop.Api.DTOs;
public class CustomerSaveRequest { [Required,MaxLength(150)] public string Name {get;set;}=""; [MaxLength(30)] public string? Phone{get;set;} [EmailAddress,MaxLength(255)] public string? Email{get;set;} [MaxLength(200)] public string? Address{get;set;} [MaxLength(100)] public string? City{get;set;} [MaxLength(100)] public string? ProvinceState{get;set;} [MaxLength(20)] public string? PostalCode{get;set;} [MaxLength(1000)] public string? Notes{get;set;} public bool IsActive{get;set;}=true; }
public class VehicleSaveRequest { [Required] public int CustomerId{get;set;} [MaxLength(17)] public string? Vin{get;set;} [Range(1886,2100)] public int? Year{get;set;} [MaxLength(100)] public string? Make{get;set;} [MaxLength(100)] public string? Model{get;set;} [MaxLength(100)] public string? Trim{get;set;} [MaxLength(30)] public string? LicensePlate{get;set;} [MaxLength(50)] public string? Color{get;set;} [Range(0,int.MaxValue)] public int? CurrentMileage{get;set;} [MaxLength(1000)] public string? Notes{get;set;} public bool IsActive{get;set;}=true; }
public class SupplierSaveRequest { [Required,MaxLength(200)] public string Name{get;set;}=""; [MaxLength(150)] public string? ContactName{get;set;} [MaxLength(30)] public string? Phone{get;set;} [EmailAddress,MaxLength(255)] public string? Email{get;set;} [MaxLength(200)] public string? Address{get;set;} [MaxLength(100)] public string? City{get;set;} [MaxLength(100)] public string? ProvinceState{get;set;} [MaxLength(20)] public string? PostalCode{get;set;} [MaxLength(1000)] public string? Notes{get;set;} public bool IsActive{get;set;}=true; }
public class RepairOrderSaveRequest
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RepairOrderNumber { get; set; } = "";

    [MaxLength(50)]
    public string? InvoiceNumber { get; set; }

    public DateTime OpenedDate { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedDate { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Open";

    public int? MileageIn { get; set; }

    public int? MileageOut { get; set; }

    [MaxLength(2000)]
    public string? CustomerConcern { get; set; }

    [MaxLength(2000)]
    public string? Diagnosis { get; set; }

    [MaxLength(2000)]
    public string? WorkPerformed { get; set; }

    [MaxLength(150)]
    public string? TechnicianName { get; set; }

    public int? AssignedTechnicianUserId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public decimal TaxAmount { get; set; }
}
public class LaborSaveRequest { [Required,MaxLength(500)] public string Description{get;set;}=""; [MaxLength(150)] public string? TechnicianName{get;set;} [MaxLength(2000)] public string? TechnicianNotes{get;set;} [Range(0.01,1000)] public decimal Hours{get;set;} [Range(0,100000)] public decimal HourlyRate{get;set;} }
public class TechnicianWorkSaveRequest { [MaxLength(2000)] public string? Diagnosis{get;set;} [MaxLength(2000)] public string? WorkPerformed{get;set;} [Range(0,int.MaxValue)] public int? MileageOut{get;set;} [Required,MaxLength(30)] public string Status{get;set;}="In Progress"; public DateTime? CompletedDate{get;set;} }
public class PartSaveRequest { public int? SupplierId{get;set;} [MaxLength(100)] public string? PartNumber{get;set;} [Required,MaxLength(300)] public string Description{get;set;}=""; [Range(0.001,100000)] public decimal Quantity{get;set;} [Range(0,1000000)] public decimal UnitCost{get;set;} [Range(0,1000000)] public decimal UnitPrice{get;set;} }
public class PaymentSaveRequest { [Range(0.01,10000000)] public decimal Amount{get;set;} public DateTime PaymentDate{get;set;}=DateTime.UtcNow; [Required,MaxLength(50)] public string PaymentMethod{get;set;}="Other"; [MaxLength(100)] public string? ReferenceNumber{get;set;} [MaxLength(500)] public string? Notes{get;set;} }
