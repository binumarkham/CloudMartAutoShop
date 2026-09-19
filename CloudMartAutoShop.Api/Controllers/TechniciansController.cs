using CloudMartAutoShop.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudMartAutoShop.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Administrator,Service Advisor")]
[Route("api/[controller]")]
public class TechniciansController(AppDbContext db) : TenantControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await db.Users
            .AsNoTracking()
            .Where(x => x.BusinessId == BusinessId && x.IsActive && x.Role == "Technician")
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Email })
            .ToListAsync();

        return Ok(items);
    }
}
