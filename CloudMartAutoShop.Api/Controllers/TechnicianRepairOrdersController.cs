using CloudMartAutoShop.Api.Data;
using CloudMartAutoShop.Api.DTOs;
using CloudMartAutoShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudMartAutoShop.Api.Controllers;

[ApiController]
[Authorize(Roles = "Technician")]
[Route("api/technician/repair-orders")]
public class TechnicianRepairOrdersController(AppDbContext db) : TenantControllerBase
{
    private static readonly string[] TechnicianStatuses =
    [
        "Open", "In Progress", "Waiting for Parts", "Completed"
    ];

    [HttpGet]
    public async Task<IActionResult> GetAssigned()
    {
        var items = await db.RepairOrders
            .AsNoTracking()
            .Where(x => x.BusinessId == BusinessId && x.AssignedTechnicianUserId == UserId)
            .OrderBy(x => x.Status == "Completed")
            .ThenByDescending(x => x.OpenedDate)
            .Select(x => new
            {
                x.Id,
                x.RepairOrderNumber,
                x.OpenedDate,
                x.CompletedDate,
                x.MileageIn,
                x.MileageOut,
                x.Status,
                CustomerName = x.Customer.Name,
                Vehicle = (x.Vehicle.Year.HasValue ? x.Vehicle.Year.Value.ToString() + " " : "") +
                          (x.Vehicle.Make ?? "") + " " + (x.Vehicle.Model ?? ""),
                x.CustomerConcern,
                x.Diagnosis,
                x.WorkPerformed,
                x.AssignedTechnicianName,
                LaborLines = x.LaborLines.OrderBy(l => l.Id).Select(l => new
                {
                    l.Id,
                    l.Description,
                    l.TechnicianName,
                    l.TechnicianNotes,
                    l.Hours,
                    l.CreatedAt
                })
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var item = await db.RepairOrders
            .AsNoTracking()
            .Where(x => x.Id == id && x.BusinessId == BusinessId && x.AssignedTechnicianUserId == UserId)
            .Select(x => new
            {
                x.Id,
                x.RepairOrderNumber,
                x.OpenedDate,
                x.CompletedDate,
                x.MileageIn,
                x.MileageOut,
                x.Status,
                CustomerName = x.Customer.Name,
                Vehicle = (x.Vehicle.Year.HasValue ? x.Vehicle.Year.Value.ToString() + " " : "") +
                          (x.Vehicle.Make ?? "") + " " + (x.Vehicle.Model ?? ""),
                x.CustomerConcern,
                x.Diagnosis,
                x.WorkPerformed,
                x.AssignedTechnicianName,
                LaborLines = x.LaborLines.OrderBy(l => l.Id).Select(l => new
                {
                    l.Id,
                    l.Description,
                    l.TechnicianName,
                    l.TechnicianNotes,
                    l.Hours,
                    l.CreatedAt
                })
            })
            .SingleOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("{id:int}/work")]
    public async Task<IActionResult> UpdateWork(int id, TechnicianWorkSaveRequest request)
    {
        if (!TechnicianStatuses.Contains(request.Status))
            return BadRequest("Invalid technician repair order status.");

        var ro = await db.RepairOrders.SingleOrDefaultAsync(x =>
            x.Id == id && x.BusinessId == BusinessId && x.AssignedTechnicianUserId == UserId);

        if (ro is null) return NotFound();

        ro.Diagnosis = Clean(request.Diagnosis);
        ro.WorkPerformed = Clean(request.WorkPerformed);
        ro.MileageOut = request.MileageOut;
        ro.Status = request.Status;
        ro.CompletedDate = request.Status == "Completed"
            ? ToUtc(request.CompletedDate ?? DateTime.Today)
            : null;
        ro.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/labor")]
    public async Task<IActionResult> AddLabor(int id, LaborSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("Labor description is required.");

        if (request.Hours <= 0)
            return BadRequest("Labor hours must be greater than zero.");

        var ro = await db.RepairOrders
            .Include(x => x.LaborLines)
            .SingleOrDefaultAsync(x =>
                x.Id == id &&
                x.BusinessId == BusinessId &&
                x.AssignedTechnicianUserId == UserId);

        if (ro is null)
            return NotFound();

        // Get the logged-in technician's current internal hourly labor rate.
        // The technician never receives or controls this value in the PWA.
        // The current rate is copied to the labor line so historical labor
        // remains unchanged if the technician's profile rate changes later.
        var technician = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == UserId &&
                x.BusinessId == BusinessId &&
                x.IsActive &&
                x.Role == "Technician");

        if (technician is null)
            return BadRequest("The technician account is not available.");

        if (!technician.HourlyRate.HasValue ||
            technician.HourlyRate.Value <= 0)
        {
            return BadRequest(
                "The technician does not have a valid hourly labor rate.");
        }

        var hourlyRate = technician.HourlyRate.Value;

        ro.LaborLines.Add(new RepairOrderLabor
        {
            BusinessId = BusinessId,
            Description = request.Description.Trim(),
            TechnicianName = technician.Name,
            TechnicianNotes = Clean(request.TechnicianNotes),
            Hours = request.Hours,

            // Snapshot the technician's current rate.
            HourlyRate = hourlyRate,

            LineTotal = request.Hours * hourlyRate,

            CreatedByUserId = UserId,
            CreatedByName = UserName,
            CreatedAt = DateTime.UtcNow
        });

        ro.LaborSubtotal =
            ro.LaborLines.Sum(x => x.LineTotal);

        ro.Subtotal =
            ro.LaborSubtotal + ro.PartsSubtotal;

        ro.TotalAmount =
            ro.Subtotal + ro.TaxAmount;

        ro.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
