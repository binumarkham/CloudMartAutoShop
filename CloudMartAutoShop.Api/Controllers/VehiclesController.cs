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
public class VehiclesController(AppDbContext db) : TenantControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await db.Vehicles
            .AsNoTracking()
            .Where(x => x.BusinessId == BusinessId)
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Customer.Name)
            .ThenByDescending(x => x.Year)
            .ThenBy(x => x.Make)
            .ThenBy(x => x.Model)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                CustomerName = x.Customer.Name,
                x.Vin,
                x.Year,
                x.Make,
                x.Model,
                x.Trim,
                x.LicensePlate,
                x.Color,
                x.CurrentMileage,
                x.Notes,
                x.IsActive,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await db.Vehicles
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.BusinessId == BusinessId)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                CustomerName = x.Customer.Name,
                x.Vin,
                x.Year,
                x.Make,
                x.Model,
                x.Trim,
                x.LicensePlate,
                x.Color,
                x.CurrentMileage,
                x.Notes,
                x.IsActive,
                x.CreatedAt
            })
            .SingleOrDefaultAsync();

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        VehicleSaveRequest request)
    {
        var validationResult =
            await ValidateRequest(
                request,
                requireActiveCustomer: true);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var vehicle = new Vehicle
        {
            BusinessId = BusinessId,
            CustomerId = request.CustomerId,
            Vin = NormalizeVin(request.Vin),
            Year = request.Year,
            Make = Clean(request.Make),
            Model = Clean(request.Model),
            Trim = Clean(request.Trim),
            LicensePlate =
                Clean(request.LicensePlate)?.ToUpperInvariant(),
            Color = Clean(request.Color),
            CurrentMileage = request.CurrentMileage,
            Notes = Clean(request.Notes),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        db.Vehicles.Add(vehicle);

        await db.SaveChangesAsync();

        return Ok(vehicle);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        VehicleSaveRequest request)
    {
        var vehicle = await db.Vehicles
            .SingleOrDefaultAsync(x =>
                x.Id == id &&
                x.BusinessId == BusinessId);

        if (vehicle is null)
        {
            return NotFound();
        }

        /*
         * If the vehicle remains assigned to its existing customer,
         * allow editing even when that customer has since become
         * inactive.
         *
         * If the customer is changed, the newly selected customer
         * must be active.
         */
        var requireActiveCustomer =
            vehicle.CustomerId != request.CustomerId;

        var validationResult =
            await ValidateRequest(
                request,
                requireActiveCustomer,
                id);

        if (validationResult is not null)
        {
            return validationResult;
        }

        vehicle.CustomerId = request.CustomerId;
        vehicle.Vin = NormalizeVin(request.Vin);
        vehicle.Year = request.Year;
        vehicle.Make = Clean(request.Make);
        vehicle.Model = Clean(request.Model);
        vehicle.Trim = Clean(request.Trim);
        vehicle.LicensePlate =
            Clean(request.LicensePlate)?.ToUpperInvariant();
        vehicle.Color = Clean(request.Color);
        vehicle.CurrentMileage = request.CurrentMileage;
        vehicle.Notes = Clean(request.Notes);
        vehicle.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return Ok(vehicle);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] VehicleStatusRequest request)
    {
        var vehicle = await db.Vehicles
            .SingleOrDefaultAsync(x =>
                x.Id == id &&
                x.BusinessId == BusinessId);

        if (vehicle is null)
        {
            return NotFound();
        }

        vehicle.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return Ok(new
        {
            vehicle.Id,
            vehicle.IsActive
        });
    }

    private async Task<BadRequestObjectResult?>
        ValidateRequest(
            VehicleSaveRequest request,
            bool requireActiveCustomer,
            int? excludeVehicleId = null)
    {
        if (request.CustomerId <= 0)
        {
            return BadRequest(
                "Please select a customer.");
        }

        var customer = await db.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == request.CustomerId &&
                x.BusinessId == BusinessId);

        if (customer is null)
        {
            return BadRequest(
                "The selected customer is invalid.");
        }

        if (requireActiveCustomer &&
            !customer.IsActive)
        {
            return BadRequest(
                "The selected customer is inactive.");
        }

        if (!request.Year.HasValue)
        {
            return BadRequest("Vehicle year is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Make))
        {
            return BadRequest("Vehicle make is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Model))
        {
            return BadRequest("Vehicle model is required.");
        }

        if (request.Year.HasValue)
        {
            var maximumYear =
                DateTime.UtcNow.Year + 1;

            if (request.Year.Value < 1886 ||
                request.Year.Value > maximumYear)
            {
                return BadRequest(
                    $"Vehicle year must be between 1886 and {maximumYear}.");
            }
        }

        if (request.CurrentMileage.HasValue &&
            request.CurrentMileage.Value < 0)
        {
            return BadRequest(
                "Mileage cannot be negative.");
        }

        var vin = NormalizeVin(request.Vin);

        /*
         * VIN is optional in the current Auto Shop model.
         * When supplied, validate it as a standard 17-character VIN.
         */
        if (!string.IsNullOrWhiteSpace(vin))
        {
            if (vin.Length != 17)
            {
                return BadRequest(
                    "VIN must contain exactly 17 characters.");
            }

            if (vin.Contains('I') ||
                vin.Contains('O') ||
                vin.Contains('Q'))
            {
                return BadRequest(
                    "VIN cannot contain the letters I, O or Q.");
            }

            var duplicateVin =
                await db.Vehicles
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.BusinessId == BusinessId &&
                        (!excludeVehicleId.HasValue ||
                         x.Id != excludeVehicleId.Value) &&
                        x.Vin == vin);

            if (duplicateVin)
            {
                return BadRequest(
                    "Another vehicle already uses this VIN.");
            }
        }

        return null;
    }

    private static string? NormalizeVin(string? value)
    {
        return Clean(value)?.ToUpperInvariant();
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}

public class VehicleStatusRequest
{
    public bool IsActive { get; set; }
}