using CloudMartAutoShop.Api.Data;
using CloudMartAutoShop.Api.DTOs;
using CloudMartAutoShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudMartAutoShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Administrator,Service Advisor")]
public class PartsController(AppDbContext db) : TenantControllerBase
{
    // GET api/Parts
    // Returns the reusable Part catalog for the current business.
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var parts = await db.Parts
            .AsNoTracking()
            .Where(x => x.BusinessId == BusinessId)
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.PartNumber)
            .ThenBy(x => x.Description)
            .Select(x => new
            {
                x.Id,
                x.PartNumber,
                x.Description,
                x.PreferredSupplierId,

                PreferredSupplierName = x.PreferredSupplier != null
                    ? x.PreferredSupplier.Name
                    : null,

                x.DefaultUnitCost,
                x.DefaultUnitPrice,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,

                SupplierCount = db.PartSuppliers.Count(ps =>
                    ps.BusinessId == BusinessId &&
                    ps.PartId == x.Id)
            })
            .ToListAsync();

        return Ok(parts);
    }

    // GET api/Parts/active
    // Lightweight list for repair-order selectors.
    [HttpGet("active")]
    public async Task<ActionResult> GetActive()
    {
        var parts = await db.Parts
            .AsNoTracking()
            .Where(x =>
                x.BusinessId == BusinessId &&
                x.IsActive)
            .OrderBy(x => x.PartNumber)
            .ThenBy(x => x.Description)
            .Select(x => new
            {
                x.Id,
                x.PartNumber,
                x.Description,
                x.PreferredSupplierId,

                PreferredSupplierName = x.PreferredSupplier != null
                    ? x.PreferredSupplier.Name
                    : null,

                x.DefaultUnitCost,
                x.DefaultUnitPrice
            })
            .ToListAsync();

        return Ok(parts);
    }

    // GET api/Parts/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult> Get(int id)
    {
        var part = await db.Parts
            .AsNoTracking()
            .Where(x =>
                x.BusinessId == BusinessId &&
                x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.PartNumber,
                x.Description,
                x.PreferredSupplierId,

                PreferredSupplierName = x.PreferredSupplier != null
                    ? x.PreferredSupplier.Name
                    : null,

                x.DefaultUnitCost,
                x.DefaultUnitPrice,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (part is null)
            return NotFound();

        return Ok(part);
    }

    // POST api/Parts
    [HttpPost]
    public async Task<ActionResult> Create(
        PartCatalogSaveRequest request)
    {
        var validation = await ValidatePartRequest(request);

        if (validation is not null)
            return validation;

        var part = new Part
        {
            BusinessId = BusinessId,
            PartNumber = Clean(request.PartNumber),
            Description = request.Description.Trim(),
            PreferredSupplierId = request.PreferredSupplierId,
            DefaultUnitCost = request.DefaultUnitCost,
            DefaultUnitPrice = request.DefaultUnitPrice,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        db.Parts.Add(part);
        await db.SaveChangesAsync();

        // If a preferred supplier was selected while creating the part,
        // automatically establish the Part <-> Supplier relationship.
        if (part.PreferredSupplierId.HasValue)
        {
            db.PartSuppliers.Add(new PartSupplier
            {
                BusinessId = BusinessId,
                PartId = part.Id,
                SupplierId = part.PreferredSupplierId.Value,
                LastCost = part.DefaultUnitCost,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = part.Id },
            new { part.Id });
    }

    // PUT api/Parts/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(
        int id,
        PartCatalogSaveRequest request)
    {
        var part = await db.Parts
            .FirstOrDefaultAsync(x =>
                x.BusinessId == BusinessId &&
                x.Id == id);

        if (part is null)
            return NotFound();

        var validation = await ValidatePartRequest(request);

        if (validation is not null)
            return validation;

        part.PartNumber = Clean(request.PartNumber);
        part.Description = request.Description.Trim();
        part.PreferredSupplierId = request.PreferredSupplierId;
        part.DefaultUnitCost = request.DefaultUnitCost;
        part.DefaultUnitPrice = request.DefaultUnitPrice;
        part.IsActive = request.IsActive;
        part.UpdatedAt = DateTime.UtcNow;

        // A preferred supplier must also exist as a supplier link.
        if (request.PreferredSupplierId.HasValue)
        {
            var supplierId = request.PreferredSupplierId.Value;

            var link = await db.PartSuppliers
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == BusinessId &&
                    x.PartId == part.Id &&
                    x.SupplierId == supplierId);

            if (link is null)
            {
                db.PartSuppliers.Add(new PartSupplier
                {
                    BusinessId = BusinessId,
                    PartId = part.Id,
                    SupplierId = supplierId,
                    LastCost = request.DefaultUnitCost,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();

        return NoContent();
    }

    // PATCH api/Parts/{id}/active
    [HttpPatch("{id:int}/active")]
    public async Task<ActionResult> SetActive(
        int id,
        [FromQuery] bool active)
    {
        var part = await db.Parts
            .FirstOrDefaultAsync(x =>
                x.BusinessId == BusinessId &&
                x.Id == id);

        if (part is null)
            return NotFound();

        part.IsActive = active;
        part.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }

    // GET api/Parts/{id}/suppliers
    [HttpGet("{id:int}/suppliers")]
    public async Task<ActionResult> GetSuppliers(int id)
    {
        var part = await db.Parts
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.BusinessId == BusinessId &&
                x.Id == id);

        if (part is null)
            return NotFound();

        var suppliers = await db.PartSuppliers
            .AsNoTracking()
            .Where(x =>
                x.BusinessId == BusinessId &&
                x.PartId == id)
            .OrderBy(x => x.Supplier.Name)
            .Select(x => new
            {
                x.Id,
                x.PartId,
                x.SupplierId,
                SupplierName = x.Supplier.Name,
                SupplierIsActive = x.Supplier.IsActive,
                x.SupplierPartNumber,
                x.LastCost,

                IsPreferred =
                    part.PreferredSupplierId == x.SupplierId,

                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    // POST api/Parts/{id}/suppliers
    // Creates or updates the supplier link.
    [HttpPost("{id:int}/suppliers")]
    public async Task<ActionResult> SaveSupplier(
        int id,
        PartSupplierSaveRequest request)
    {
        var part = await db.Parts
            .FirstOrDefaultAsync(x =>
                x.BusinessId == BusinessId &&
                x.Id == id);

        if (part is null)
            return NotFound("Part was not found.");

        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(x =>
                x.BusinessId == BusinessId &&
                x.Id == request.SupplierId);

        if (supplier is null)
            return BadRequest(
                "The selected supplier does not belong to this business.");

        if (!supplier.IsActive)
            return BadRequest(
                "The selected supplier is inactive.");

        var link = await db.PartSuppliers
            .FirstOrDefaultAsync(x =>
                x.BusinessId == BusinessId &&
                x.PartId == id &&
                x.SupplierId == request.SupplierId);

        if (link is null)
        {
            link = new PartSupplier
            {
                BusinessId = BusinessId,
                PartId = id,
                SupplierId = request.SupplierId,
                SupplierPartNumber =
                    Clean(request.SupplierPartNumber),
                LastCost = request.LastCost,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.PartSuppliers.Add(link);
        }
        else
        {
            link.SupplierPartNumber =
                Clean(request.SupplierPartNumber);

            link.LastCost = request.LastCost;
            link.UpdatedAt = DateTime.UtcNow;
        }

        if (request.IsPreferred)
        {
            part.PreferredSupplierId = request.SupplierId;

            // Keep the master default cost synchronized with
            // the preferred supplier's most recent known cost.
            if (request.LastCost.HasValue)
                part.DefaultUnitCost = request.LastCost.Value;

            part.UpdatedAt = DateTime.UtcNow;
        }
        else if (part.PreferredSupplierId == request.SupplierId)
        {
            // The caller explicitly removed Preferred from
            // the currently preferred supplier.
            part.PreferredSupplierId = null;
            part.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        return Ok(new
        {
            link.Id,
            link.PartId,
            link.SupplierId,
            link.SupplierPartNumber,
            link.LastCost,
            IsPreferred =
                part.PreferredSupplierId == link.SupplierId
        });
    }

    // DELETE api/Parts/{partId}/suppliers/{supplierId}
    [HttpDelete("{partId:int}/suppliers/{supplierId:int}")]
    public async Task<ActionResult> RemoveSupplier(
        int partId,
        int supplierId)
    {
        var part = await db.Parts
            .FirstOrDefaultAsync(x =>
                x.BusinessId == BusinessId &&
                x.Id == partId);

        if (part is null)
            return NotFound("Part was not found.");

        var link = await db.PartSuppliers
            .FirstOrDefaultAsync(x =>
                x.BusinessId == BusinessId &&
                x.PartId == partId &&
                x.SupplierId == supplierId);

        if (link is null)
            return NotFound("Supplier link was not found.");

        if (part.PreferredSupplierId == supplierId)
        {
            part.PreferredSupplierId = null;
            part.UpdatedAt = DateTime.UtcNow;
        }

        db.PartSuppliers.Remove(link);

        await db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult?> ValidatePartRequest(
        PartCatalogSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("Part description is required.");

        if (request.PreferredSupplierId.HasValue)
        {
            var supplierExists = await db.Suppliers
                .AnyAsync(x =>
                    x.BusinessId == BusinessId &&
                    x.Id == request.PreferredSupplierId.Value &&
                    x.IsActive);

            if (!supplierExists)
            {
                return BadRequest(
                    "The preferred supplier is invalid or inactive.");
            }
        }

        return null;
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}