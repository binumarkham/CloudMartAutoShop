using CloudMartAutoShop.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudMartAutoShop.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Administrator,Service Advisor")]
[Route("api/[controller]")]
public class ReportsController(AppDbContext db) : TenantControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var ro = db.RepairOrders.Where(x => x.BusinessId == BusinessId && x.Status != "Cancelled");
        return Ok(new { OpenRepairOrders = await ro.CountAsync(x => x.Status != "Completed" && x.Status != "Invoiced"), TotalSales = await ro.SumAsync(x => (decimal?)x.TotalAmount) ?? 0, Outstanding = await ro.SumAsync(x => (decimal?)(x.TotalAmount - x.AmountPaid)) ?? 0, Customers = await db.Customers.CountAsync(x => x.BusinessId == BusinessId && x.IsActive), Vehicles = await db.Vehicles.CountAsync(x => x.BusinessId == BusinessId && x.IsActive) });
    }

    [HttpGet("outstanding")]
    public async Task<IActionResult> Outstanding() => Ok(await db.RepairOrders.AsNoTracking()
        .Where(x => x.BusinessId == BusinessId && x.Status != "Cancelled" && x.TotalAmount > x.AmountPaid)
        .OrderByDescending(x => x.OpenedDate)
        .Select(x => new { x.RepairOrderNumber, x.InvoiceNumber, Customer = x.Customer.Name, x.OpenedDate, x.TotalAmount, x.AmountPaid, Balance = x.TotalAmount - x.AmountPaid }).ToListAsync());

    [HttpGet("repair-orders")]
    public async Task<IActionResult> RepairOrders(DateTime? from = null, DateTime? to = null, string? status = null)
    {
        var q = db.RepairOrders.AsNoTracking().Where(x => x.BusinessId == BusinessId);
        if (from.HasValue) q = q.Where(x => x.OpenedDate >= UtcStart(from.Value));
        if (to.HasValue) q = q.Where(x => x.OpenedDate < UtcStart(to.Value).AddDays(1));
        if (!string.IsNullOrWhiteSpace(status) && status != "All") q = q.Where(x => x.Status == status);
        return Ok(await q.OrderByDescending(x => x.OpenedDate).Select(x => new { x.RepairOrderNumber, x.InvoiceNumber, Customer = x.Customer.Name, Vehicle = (x.Vehicle.Year.HasValue ? x.Vehicle.Year.Value.ToString() + " " : "") + (x.Vehicle.Make ?? "") + " " + (x.Vehicle.Model ?? ""), x.OpenedDate, x.Status, x.TotalAmount, x.AmountPaid, Balance = x.TotalAmount - x.AmountPaid }).ToListAsync());
    }

    [HttpGet("sales")]
    public async Task<IActionResult> Sales(DateTime? from = null, DateTime? to = null)
    {
        var q = db.RepairOrders.AsNoTracking().Where(x => x.BusinessId == BusinessId && x.Status != "Cancelled");
        if (from.HasValue) q = q.Where(x => x.OpenedDate >= UtcStart(from.Value));
        if (to.HasValue) q = q.Where(x => x.OpenedDate < UtcStart(to.Value).AddDays(1));
        var rows = await q.OrderByDescending(x => x.OpenedDate).Select(x => new { x.RepairOrderNumber, Customer = x.Customer.Name, x.OpenedDate, x.LaborSubtotal, x.PartsSubtotal, x.TaxAmount, x.TotalAmount, x.AmountPaid }).ToListAsync();
        return Ok(new { TotalLabor = rows.Sum(x => x.LaborSubtotal), TotalParts = rows.Sum(x => x.PartsSubtotal), TotalTax = rows.Sum(x => x.TaxAmount), TotalSales = rows.Sum(x => x.TotalAmount), TotalPaid = rows.Sum(x => x.AmountPaid), Rows = rows });
    }

    [HttpGet("payments")]
    public async Task<IActionResult> Payments(DateTime? from = null, DateTime? to = null)
    {
        var q = db.Payments.AsNoTracking().Where(x => x.BusinessId == BusinessId);
        if (from.HasValue) q = q.Where(x => x.PaymentDate >= UtcStart(from.Value));
        if (to.HasValue) q = q.Where(x => x.PaymentDate < UtcStart(to.Value).AddDays(1));
        return Ok(await q.OrderByDescending(x => x.PaymentDate).Select(x => new { x.PaymentDate, x.RepairOrder.RepairOrderNumber, Customer = x.RepairOrder.Customer.Name, x.Amount, x.PaymentMethod, x.ReferenceNumber }).ToListAsync());
    }

    [HttpGet("parts")]
    public async Task<IActionResult> Parts(DateTime? from = null, DateTime? to = null)
    {
        var q = db.RepairOrderParts.AsNoTracking().Where(x => x.BusinessId == BusinessId && x.RepairOrder.Status != "Cancelled");
        if (from.HasValue) q = q.Where(x => x.RepairOrder.OpenedDate >= UtcStart(from.Value));
        if (to.HasValue) q = q.Where(x => x.RepairOrder.OpenedDate < UtcStart(to.Value).AddDays(1));
        return Ok(await q.OrderByDescending(x => x.CreatedAt).Select(x => new { Supplier = x.Supplier != null ? x.Supplier.Name : null, x.RepairOrder.RepairOrderNumber, x.PartNumber, x.Description, x.Quantity, x.UnitCost, x.UnitPrice, x.LineTotal, x.CreatedAt }).ToListAsync());
    }

    [HttpGet("technician-hours")]
    public async Task<IActionResult> TechnicianHours(DateTime? from = null, DateTime? to = null)
    {
        var q = db.RepairOrderLabors.AsNoTracking().Where(x => x.BusinessId == BusinessId && x.RepairOrder.Status != "Cancelled");
        if (from.HasValue) q = q.Where(x => x.RepairOrder.OpenedDate >= UtcStart(from.Value));
        if (to.HasValue) q = q.Where(x => x.RepairOrder.OpenedDate < UtcStart(to.Value).AddDays(1));
        return Ok(await q.GroupBy(x => x.TechnicianName ?? x.CreatedByName ?? "Unassigned").Select(g => new { Technician = g.Key, Hours = g.Sum(x => x.Hours), LaborAmount = g.Sum(x => x.LineTotal), Entries = g.Count() }).OrderByDescending(x => x.Hours).ToListAsync());
    }

    private static DateTime UtcStart(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
