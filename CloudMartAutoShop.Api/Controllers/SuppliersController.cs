using CloudMartAutoShop.Api.Data;
using CloudMartAutoShop.Api.DTOs;
using CloudMartAutoShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudMartAutoShop.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Administrator,Service Advisor")]
[Route("api/[controller]")]
public class SuppliersController(AppDbContext db) : TenantControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await db.Suppliers
        .AsNoTracking().Where(x => x.BusinessId == BusinessId)
        .OrderBy(x => x.Name)
        .Select(x => new { x.Id, x.Name, x.ContactName, x.Phone, x.Email, x.Address, x.City, x.ProvinceState, x.PostalCode, x.Notes, x.IsActive, x.CreatedAt })
        .ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(SupplierSaveRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest("Supplier name is required.");
        var x = new Supplier { BusinessId = BusinessId, Name = r.Name.Trim(), ContactName = Clean(r.ContactName), Phone = Clean(r.Phone), Email = Clean(r.Email), Address = Clean(r.Address), City = Clean(r.City), ProvinceState = Clean(r.ProvinceState), PostalCode = Clean(r.PostalCode), Notes = Clean(r.Notes), IsActive = r.IsActive, CreatedAt = DateTime.UtcNow };
        db.Suppliers.Add(x); await db.SaveChangesAsync();
        return Ok(new { x.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SupplierSaveRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest("Supplier name is required.");
        var x = await db.Suppliers.SingleOrDefaultAsync(x => x.Id == id && x.BusinessId == BusinessId);
        if (x is null) return NotFound();
        x.Name = r.Name.Trim(); x.ContactName = Clean(r.ContactName); x.Phone = Clean(r.Phone); x.Email = Clean(r.Email); x.Address = Clean(r.Address); x.City = Clean(r.City); x.ProvinceState = Clean(r.ProvinceState); x.PostalCode = Clean(r.PostalCode); x.Notes = Clean(r.Notes); x.IsActive = r.IsActive;
        await db.SaveChangesAsync();
        return Ok(new { x.Id });
    }

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, ActiveRequest r)
    {
        var x = await db.Suppliers.SingleOrDefaultAsync(x => x.Id == id && x.BusinessId == BusinessId);
        if (x is null) return NotFound();
        x.IsActive = r.IsActive; await db.SaveChangesAsync();
        return Ok(new { x.Id, x.IsActive });
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    public sealed class ActiveRequest { public bool IsActive { get; set; } }
}
