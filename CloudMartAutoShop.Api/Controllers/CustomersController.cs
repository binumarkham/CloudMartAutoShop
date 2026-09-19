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
public class CustomersController(AppDbContext db) : TenantControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await db.Customers
            .AsNoTracking()
            .Where(x => x.BusinessId == BusinessId)
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Phone,
                x.Email,
                x.Address,
                x.City,
                x.ProvinceState,
                x.PostalCode,
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
        var item = await db.Customers
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.BusinessId == BusinessId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Phone,
                x.Email,
                x.Address,
                x.City,
                x.ProvinceState,
                x.PostalCode,
                x.Notes,
                x.IsActive,
                x.CreatedAt
            })
            .SingleOrDefaultAsync();

        return item is null
            ? NotFound()
            : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CustomerSaveRequest request)
    {
        var validationResult = ValidateRequest(request);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var customer = new Customer
        {
            BusinessId = BusinessId,
            Name = request.Name.Trim(),
            Phone = Clean(request.Phone),
            Email = Clean(request.Email)?.ToLowerInvariant(),
            Address = Clean(request.Address),
            City = Clean(request.City),
            ProvinceState = Clean(request.ProvinceState),
            PostalCode = Clean(request.PostalCode)?.ToUpperInvariant(),
            Notes = Clean(request.Notes),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        return Ok(customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        CustomerSaveRequest request)
    {
        var validationResult = ValidateRequest(request);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var customer = await db.Customers
            .SingleOrDefaultAsync(x =>
                x.Id == id &&
                x.BusinessId == BusinessId);

        if (customer is null)
        {
            return NotFound();
        }

        customer.Name = request.Name.Trim();
        customer.Phone = Clean(request.Phone);
        customer.Email =
            Clean(request.Email)?.ToLowerInvariant();
        customer.Address = Clean(request.Address);
        customer.City = Clean(request.City);
        customer.ProvinceState =
            Clean(request.ProvinceState);
        customer.PostalCode =
            Clean(request.PostalCode)?.ToUpperInvariant();
        customer.Notes = Clean(request.Notes);
        customer.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return Ok(customer);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] CustomerStatusRequest request)
    {
        var customer = await db.Customers
            .SingleOrDefaultAsync(x =>
                x.Id == id &&
                x.BusinessId == BusinessId);

        if (customer is null)
        {
            return NotFound();
        }

        customer.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return Ok(new
        {
            customer.Id,
            customer.IsActive
        });
    }

    private BadRequestObjectResult? ValidateRequest(
        CustomerSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Customer name is required.");
        }

        if (request.Name.Trim().Length > 150)
        {
            return BadRequest(
                "Customer name cannot exceed 150 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                .IsValid(request.Email.Trim()))
        {
            return BadRequest(
                "Please enter a valid email address.");
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

public class CustomerStatusRequest
{
    public bool IsActive { get; set; }
}