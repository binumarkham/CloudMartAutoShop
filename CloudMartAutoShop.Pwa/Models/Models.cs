namespace CloudMartAutoShop.Pwa.Models;

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string BusinessName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}

public class SummaryDto
{
    public int OpenRepairOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Outstanding { get; set; }
    public int Customers { get; set; }
    public int Vehicles { get; set; }
}

public class CustomerDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? ProvinceState { get; set; }

    public string? PostalCode { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}

public class VehicleDto
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = "";

    public string? Vin { get; set; }

    public int? Year { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    public string? Trim { get; set; }

    public string? LicensePlate { get; set; }

    public string? Color { get; set; }

    public int? CurrentMileage { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}

public class RepairOrderDto
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int VehicleId { get; set; }

    public string RepairOrderNumber { get; set; } = "";

    public string? InvoiceNumber { get; set; }

    public DateTime OpenedDate { get; set; } = DateTime.Today;

    public DateTime? CompletedDate { get; set; }

    public int? MileageIn { get; set; }

    public int? MileageOut { get; set; }

    public string Status { get; set; } = "Open";

    public string CustomerName { get; set; } = "";

    public string Vehicle { get; set; } = "";

    public string? CustomerConcern { get; set; }

    public string? Diagnosis { get; set; }

    public string? WorkPerformed { get; set; }

    public string? TechnicianName { get; set; }

    public int? AssignedTechnicianUserId { get; set; }

    public string? AssignedTechnicianName { get; set; }

    public decimal LaborSubtotal { get; set; }

    public decimal PartsSubtotal { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal Balance { get; set; }

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/*
 * Detailed Repair Order response.
 *
 * GET api/RepairOrders/{id} returns the Repair Order
 * together with its Labor, Part and Payment collections.
 *
 * We are adding Labor now. Parts and Payments will be
 * added to this class when we implement those screens.
 */
public class RepairOrderDetailDto : RepairOrderDto
{
    public List<RepairOrderLaborDto> LaborLines { get; set; } = [];
    public List<RepairOrderPartDto> PartLines { get; set; } = [];
    public List<PaymentDto> Payments { get; set; } = [];
}

/*
 * Individual Labor line returned by:
 *
 * GET api/RepairOrders/{id}
 */
public class RepairOrderLaborDto
{
    public int Id { get; set; }

    public string Description { get; set; } = "";

    public string? TechnicianName { get; set; }

    public string? TechnicianNotes { get; set; }

    public decimal Hours { get; set; }

    public decimal HourlyRate { get; set; }

    public decimal LineTotal { get; set; }

    public int? CreatedByUserId { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedAt { get; set; }
}

/*
 * Request sent to:
 *
 * POST api/RepairOrders/{id}/labor
 */
public class LaborSaveRequest
{
    public string Description { get; set; } = "";

    public string? TechnicianName { get; set; }

    public string? TechnicianNotes { get; set; }

    public decimal Hours { get; set; } = 1m;

    public decimal HourlyRate { get; set; }
}

public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ProvinceState { get; set; }
    public string? PostalCode { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class SupplierSaveRequest
{
    public string Name { get; set; } = "";
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ProvinceState { get; set; }
    public string? PostalCode { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class OutstandingDto
{
    public string RepairOrderNumber { get; set; } = "";

    public string? InvoiceNumber { get; set; }

    public string Customer { get; set; } = "";

    public DateTime OpenedDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal Balance { get; set; }
}

public class TechnicianDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

public class TechnicianRepairOrderDto
{
    public int Id { get; set; }
    public string RepairOrderNumber { get; set; } = "";
    public DateTime OpenedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? MileageIn { get; set; }
    public int? MileageOut { get; set; }
    public string Status { get; set; } = "Open";
    public string CustomerName { get; set; } = "";
    public string Vehicle { get; set; } = "";
    public string? CustomerConcern { get; set; }
    public string? Diagnosis { get; set; }
    public string? WorkPerformed { get; set; }
    public string? AssignedTechnicianName { get; set; }
    public List<TechnicianLaborDto> LaborLines { get; set; } = [];
}

public class TechnicianLaborDto
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public string? TechnicianName { get; set; }
    public string? TechnicianNotes { get; set; }
    public decimal Hours { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TechnicianWorkSaveRequest
{
    public string? Diagnosis { get; set; }
    public string? WorkPerformed { get; set; }
    public int? MileageOut { get; set; }
    public string Status { get; set; } = "In Progress";
    public DateTime? CompletedDate { get; set; }
}

public class UserAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "Technician";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class UserSaveRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "Technician";
    public string? Password { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RepairOrderPartDto { public int Id { get; set; } public int? SupplierId { get; set; } public string? SupplierName { get; set; } public string? PartNumber { get; set; } public string Description { get; set; } = ""; public decimal Quantity { get; set; } public decimal UnitCost { get; set; } public decimal UnitPrice { get; set; } public decimal LineTotal { get; set; } public DateTime CreatedAt { get; set; } }
public class PartSaveRequest { public int? SupplierId { get; set; } public string? PartNumber { get; set; } public string Description { get; set; } = ""; public decimal Quantity { get; set; } = 1m; public decimal UnitCost { get; set; } public decimal UnitPrice { get; set; } }
public class PartCatalogDto { public int Id { get; set; } public int? SupplierId { get; set; } public string? SupplierName { get; set; } public string? PartNumber { get; set; } public string Description { get; set; } = ""; public decimal UnitCost { get; set; } public decimal UnitPrice { get; set; } }
public class PaymentDto { public int Id { get; set; } public decimal Amount { get; set; } public DateTime PaymentDate { get; set; } public string PaymentMethod { get; set; } = ""; public string? ReferenceNumber { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } }
public class RepairOrderReportDto { public string RepairOrderNumber { get; set; } = ""; public string? InvoiceNumber { get; set; } public string Customer { get; set; } = ""; public string Vehicle { get; set; } = ""; public DateTime OpenedDate { get; set; } public string Status { get; set; } = ""; public decimal TotalAmount { get; set; } public decimal AmountPaid { get; set; } public decimal Balance { get; set; } }
public class SalesReportDto { public decimal TotalLabor { get; set; } public decimal TotalParts { get; set; } public decimal TotalTax { get; set; } public decimal TotalSales { get; set; } public decimal TotalPaid { get; set; } public List<SalesRowDto> Rows { get; set; } = []; }
public class SalesRowDto { public string RepairOrderNumber { get; set; } = ""; public string Customer { get; set; } = ""; public DateTime OpenedDate { get; set; } public decimal LaborSubtotal { get; set; } public decimal PartsSubtotal { get; set; } public decimal TaxAmount { get; set; } public decimal TotalAmount { get; set; } public decimal AmountPaid { get; set; } }
public class PaymentReportDto { public DateTime PaymentDate { get; set; } public string RepairOrderNumber { get; set; } = ""; public string Customer { get; set; } = ""; public decimal Amount { get; set; } public string PaymentMethod { get; set; } = ""; public string? ReferenceNumber { get; set; } }
public class PartsReportDto { public string? Supplier { get; set; } public string RepairOrderNumber { get; set; } = ""; public string? PartNumber { get; set; } public string Description { get; set; } = ""; public decimal Quantity { get; set; } public decimal UnitCost { get; set; } public decimal UnitPrice { get; set; } public decimal LineTotal { get; set; } public DateTime CreatedAt { get; set; } }
public class TechnicianHoursReportDto { public string Technician { get; set; } = ""; public decimal Hours { get; set; } public decimal LaborAmount { get; set; } public int Entries { get; set; } }
