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
public class RepairOrdersController(
    AppDbContext db) : TenantControllerBase
{
    private static readonly string[] Statuses =
    [
        "Open",
        "In Progress",
        "Waiting for Parts",
        "Completed",
        "Invoiced",
        "Cancelled"
    ];

    private static readonly string[] PaymentMethods =
    [
        "Cash",
        "Debit",
        "Credit Card",
        "E-Transfer",
        "Cheque",
        "Other"
    ];

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await db.RepairOrders
            .AsNoTracking()
            .Where(x => x.BusinessId == BusinessId)
            .OrderByDescending(x => x.OpenedDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                x.VehicleId,
                x.RepairOrderNumber,
                x.InvoiceNumber,
                x.OpenedDate,
                x.CompletedDate,
                x.MileageIn,
                x.MileageOut,
                x.Status,

                CustomerName = x.Customer.Name,

                Vehicle =
                    (x.Vehicle.Year.HasValue
                        ? x.Vehicle.Year.Value.ToString() + " "
                        : "") +
                    (x.Vehicle.Make ?? "") + " " +
                    (x.Vehicle.Model ?? ""),

                x.CustomerConcern,
                x.Diagnosis,
                x.WorkPerformed,
                x.TechnicianName,
                x.AssignedTechnicianUserId,
                x.AssignedTechnicianName,

                x.LaborSubtotal,
                x.PartsSubtotal,
                x.Subtotal,
                x.TaxAmount,
                x.TotalAmount,
                x.AmountPaid,

                Balance =
                    x.TotalAmount - x.AmountPaid,

                x.Notes,
                x.CreatedByUserId,
                x.CreatedByName,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var item = await db.RepairOrders
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.BusinessId == BusinessId)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                x.VehicleId,
                x.RepairOrderNumber,
                x.InvoiceNumber,
                x.OpenedDate,
                x.CompletedDate,
                x.MileageIn,
                x.MileageOut,
                x.Status,

                CustomerName = x.Customer.Name,

                Vehicle =
                    (x.Vehicle.Year.HasValue
                        ? x.Vehicle.Year.Value.ToString() + " "
                        : "") +
                    (x.Vehicle.Make ?? "") + " " +
                    (x.Vehicle.Model ?? ""),

                x.CustomerConcern,
                x.Diagnosis,
                x.WorkPerformed,
                x.TechnicianName,
                x.AssignedTechnicianUserId,
                x.AssignedTechnicianName,

                x.LaborSubtotal,
                x.PartsSubtotal,
                x.Subtotal,
                x.TaxAmount,
                x.TotalAmount,
                x.AmountPaid,

                Balance =
                    x.TotalAmount - x.AmountPaid,

                x.Notes,
                x.CreatedByUserId,
                x.CreatedByName,
                x.CreatedAt,
                x.UpdatedAt,

                LaborLines = x.LaborLines
                    .OrderBy(l => l.Id)
                    .Select(l => new
                    {
                        l.Id,
                        l.Description,
                        l.TechnicianName,
                        l.TechnicianNotes,
                        l.Hours,
                        l.HourlyRate,
                        l.LineTotal,
                        l.CreatedByUserId,
                        l.CreatedByName,
                        l.CreatedAt
                    }),

                PartLines = x.PartLines
                    .OrderBy(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.SupplierId,
                        SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                        p.PartNumber,
                        p.Description,
                        p.Quantity,
                        p.UnitCost,
                        p.UnitPrice,
                        p.LineTotal,
                        p.CreatedByUserId,
                        p.CreatedByName,
                        p.CreatedAt
                    }),

                Payments = x.Payments
                    .OrderByDescending(p => p.PaymentDate)
                    .ThenByDescending(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Amount,
                        p.PaymentDate,
                        p.PaymentMethod,
                        p.ReferenceNumber,
                        p.Notes,
                        p.CreatedByUserId,
                        p.CreatedByName,
                        p.CreatedAt
                    })
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
        RepairOrderSaveRequest request)
    {
        var validation =
            await ValidateRepairOrder(
                request,
                excludeRepairOrderId: null,
                requireActiveCustomerAndVehicle: true);

        if (validation is not null)
        {
            return validation;
        }

        var repairOrder = new RepairOrder
        {
            BusinessId = BusinessId,

            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,

            RepairOrderNumber =
                request.RepairOrderNumber.Trim(),

            InvoiceNumber =
    Clean(request.InvoiceNumber),

            OpenedDate =
    ToUtc(request.OpenedDate),

            CompletedDate =
    GetCompletedDate(
        request.Status,
        request.CompletedDate),

            MileageIn = request.MileageIn,
            MileageOut = request.MileageOut,

            Status = request.Status,

            CustomerConcern =
                Clean(request.CustomerConcern),

            Diagnosis =
                Clean(request.Diagnosis),

            WorkPerformed =
                Clean(request.WorkPerformed),

            TechnicianName =
                Clean(request.TechnicianName),

            AssignedTechnicianUserId = request.AssignedTechnicianUserId,
            AssignedTechnicianName = await GetTechnicianName(request.AssignedTechnicianUserId),

            Notes =
                Clean(request.Notes),

            /*
             * Labor/parts totals are controlled by their
             * individual line items.
             */
            LaborSubtotal = 0m,
            PartsSubtotal = 0m,
            Subtotal = 0m,

            TaxAmount =
                request.TaxAmount < 0
                    ? 0
                    : request.TaxAmount,

            TotalAmount =
                request.TaxAmount < 0
                    ? 0
                    : request.TaxAmount,

            AmountPaid = 0m,

            CreatedByUserId = UserId,
            CreatedByName = UserName,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.RepairOrders.Add(repairOrder);

        await db.SaveChangesAsync();

        return Ok(new { repairOrder.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        RepairOrderSaveRequest request)
    {
        var repairOrder = await Find(id);

        if (repairOrder is null)
        {
            return NotFound();
        }

        /*
         * An old RO remains editable if its current customer/
         * vehicle later became inactive. If the relationship is
         * changed, however, the newly selected records must be
         * active.
         */
        var relationshipChanged =
            repairOrder.CustomerId != request.CustomerId ||
            repairOrder.VehicleId != request.VehicleId;

        var validation =
            await ValidateRepairOrder(
                request,
                excludeRepairOrderId: id,
                requireActiveCustomerAndVehicle:
                    relationshipChanged);

        if (validation is not null)
        {
            return validation;
        }

        repairOrder.CustomerId =
            request.CustomerId;

        repairOrder.VehicleId =
            request.VehicleId;

        repairOrder.RepairOrderNumber =
            request.RepairOrderNumber.Trim();

        repairOrder.InvoiceNumber =
            Clean(request.InvoiceNumber);

        repairOrder.OpenedDate =
    ToUtc(request.OpenedDate);

        repairOrder.CompletedDate =
            GetCompletedDate(
                request.Status,
                request.CompletedDate);

        repairOrder.MileageIn =
            request.MileageIn;

        repairOrder.MileageOut =
            request.MileageOut;

        repairOrder.Status =
            request.Status;

        repairOrder.CustomerConcern =
            Clean(request.CustomerConcern);

        repairOrder.Diagnosis =
            Clean(request.Diagnosis);

        repairOrder.WorkPerformed =
            Clean(request.WorkPerformed);

        repairOrder.TechnicianName =
            Clean(request.TechnicianName);

        repairOrder.AssignedTechnicianUserId = request.AssignedTechnicianUserId;
        repairOrder.AssignedTechnicianName = await GetTechnicianName(request.AssignedTechnicianUserId);

        repairOrder.Notes =
            Clean(request.Notes);

        repairOrder.TaxAmount =
            request.TaxAmount < 0
                ? 0
                : request.TaxAmount;

        await Recalculate(repairOrder);
        return Ok(new { repairOrder.Id });
    }

    [HttpPost("{id:int}/labor")]
    public async Task<IActionResult> AddLabor(
        int id,
        LaborSaveRequest request)
    {
        var repairOrder =
            await Find(id);

        if (repairOrder is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(
            request.Description))
        {
            return BadRequest(
                "Labor description is required.");
        }

        if (request.Hours <= 0)
        {
            return BadRequest(
                "Labor hours must be greater than zero.");
        }

        if (request.HourlyRate < 0)
        {
            return BadRequest(
                "Hourly rate cannot be negative.");
        }

        repairOrder.LaborLines.Add(
            new RepairOrderLabor
            {
                BusinessId = BusinessId,

                Description =
                    request.Description.Trim(),

                TechnicianName =
                    Clean(request.TechnicianName),

                TechnicianNotes =
                    Clean(request.TechnicianNotes),

                Hours = request.Hours,
                HourlyRate = request.HourlyRate,

                LineTotal =
                    request.Hours *
                    request.HourlyRate,

                CreatedByUserId = UserId,
                CreatedByName = UserName,

                CreatedAt = DateTime.UtcNow
            });

        await Recalculate(repairOrder);

        return Ok(new { repairOrder.Id });
    }

    [HttpGet("parts/catalog")]
    public async Task<IActionResult> GetPartsCatalog()
    {
        var parts = await db.RepairOrderParts
            .AsNoTracking()
            .Where(x => x.BusinessId == BusinessId)
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SupplierId,
                SupplierName = x.Supplier != null ? x.Supplier.Name : null,
                x.PartNumber,
                x.Description,
                x.UnitCost,
                x.UnitPrice
            })
            .ToListAsync();

        var catalog = parts
            .GroupBy(x => string.IsNullOrWhiteSpace(x.PartNumber)
                ? "DESC:" + x.Description.Trim().ToUpperInvariant()
                : "PART:" + x.PartNumber.Trim().ToUpperInvariant())
            .Select(g => g.First())
            .OrderBy(x => x.PartNumber ?? x.Description)
            .ToList();

        return Ok(catalog);
    }

    [HttpPut("{id:int}/parts/{partId:int}")]
    public async Task<IActionResult> UpdatePart(int id, int partId, PartSaveRequest request)
    {
        var repairOrder = await Find(id);
        if (repairOrder is null) return NotFound();

        var line = repairOrder.PartLines.SingleOrDefault(x => x.Id == partId && x.BusinessId == BusinessId);
        if (line is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Description)) return BadRequest("Part description is required.");
        if (request.Quantity <= 0) return BadRequest("Part quantity must be greater than zero.");
        if (request.UnitCost < 0 || request.UnitPrice < 0) return BadRequest("Part cost and price cannot be negative.");

        if (request.SupplierId.HasValue)
        {
            var supplierExists = await db.Suppliers.AnyAsync(x =>
                x.Id == request.SupplierId.Value && x.BusinessId == BusinessId && x.IsActive);
            if (!supplierExists) return BadRequest("The selected supplier is invalid or inactive.");
        }

        line.SupplierId = request.SupplierId;
        line.PartNumber = Clean(request.PartNumber);
        line.Description = request.Description.Trim();
        line.Quantity = request.Quantity;
        line.UnitCost = request.UnitCost;
        line.UnitPrice = request.UnitPrice;
        line.LineTotal = request.Quantity * request.UnitPrice;

        await Recalculate(repairOrder);
        return Ok(new { repairOrder.Id, PartId = line.Id });
    }

    [HttpPost("{id:int}/parts")]
    public async Task<IActionResult> AddPart(
        int id,
        PartSaveRequest request)
    {
        var repairOrder =
            await Find(id);

        if (repairOrder is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(
            request.Description))
        {
            return BadRequest(
                "Part description is required.");
        }

        if (request.Quantity <= 0)
        {
            return BadRequest(
                "Part quantity must be greater than zero.");
        }

        if (request.UnitCost < 0)
        {
            return BadRequest(
                "Unit cost cannot be negative.");
        }

        if (request.UnitPrice < 0)
        {
            return BadRequest(
                "Unit price cannot be negative.");
        }

        if (request.SupplierId.HasValue)
        {
            var supplierExists = await db.Suppliers.AnyAsync(x =>
                x.Id == request.SupplierId.Value &&
                x.BusinessId == BusinessId &&
                x.IsActive);

            if (!supplierExists)
            {
                return BadRequest("The selected supplier is invalid or inactive.");
            }
        }

        repairOrder.PartLines.Add(
            new RepairOrderPart
            {
                BusinessId = BusinessId,

                SupplierId = request.SupplierId,

                PartNumber =
                    Clean(request.PartNumber),

                Description =
                    request.Description.Trim(),

                Quantity =
                    request.Quantity,

                UnitCost =
                    request.UnitCost,

                UnitPrice =
                    request.UnitPrice,

                LineTotal =
                    request.Quantity *
                    request.UnitPrice,

                CreatedByUserId = UserId,
                CreatedByName = UserName,

                CreatedAt = DateTime.UtcNow
            });

        await Recalculate(repairOrder);

        return Ok(new { repairOrder.Id });
    }

    [HttpPost("{id:int}/payments")]
    public async Task<IActionResult> AddPayment(
        int id,
        PaymentSaveRequest request)
    {
        if (!PaymentMethods.Contains(
            request.PaymentMethod))
        {
            return BadRequest(
                "Invalid payment method.");
        }

        if (request.Amount <= 0)
        {
            return BadRequest(
                "Payment amount must be greater than zero.");
        }

        var repairOrder =
            await Find(id);

        if (repairOrder is null)
        {
            return NotFound();
        }

        repairOrder.Payments.Add(
            new Payment
            {
                BusinessId = BusinessId,

                Amount =
                    request.Amount,

                PaymentDate =
    ToUtc(request.PaymentDate),

                PaymentMethod =
                    request.PaymentMethod,

                ReferenceNumber =
                    Clean(request.ReferenceNumber),

                Notes =
                    Clean(request.Notes),

                CreatedByUserId = UserId,
                CreatedByName = UserName,

                CreatedAt = DateTime.UtcNow
            });

        await Recalculate(repairOrder);

        return Ok(new { repairOrder.Id });
    }

    private async Task<RepairOrder?> Find(
        int id)
    {
        return await db.RepairOrders
            .Include(x => x.LaborLines)
            .Include(x => x.PartLines)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(x =>
                x.Id == id &&
                x.BusinessId == BusinessId);
    }

    private async Task<BadRequestObjectResult?>
        ValidateRepairOrder(
            RepairOrderSaveRequest request,
            int? excludeRepairOrderId,
            bool requireActiveCustomerAndVehicle)
    {
        if (request.CustomerId <= 0)
        {
            return BadRequest(
                "Please select a customer.");
        }

        if (request.VehicleId <= 0)
        {
            return BadRequest(
                "Please select a vehicle.");
        }

        if (string.IsNullOrWhiteSpace(
            request.RepairOrderNumber))
        {
            return BadRequest(
                "Repair order number is required.");
        }

        if (!Statuses.Contains(
            request.Status))
        {
            return BadRequest(
                "Invalid repair order status.");
        }

        if (request.MileageIn.HasValue &&
            request.MileageIn.Value < 0)
        {
            return BadRequest(
                "Mileage In cannot be negative.");
        }

        if (request.MileageOut.HasValue &&
            request.MileageOut.Value < 0)
        {
            return BadRequest(
                "Mileage Out cannot be negative.");
        }

        if (request.TaxAmount < 0)
        {
            return BadRequest(
                "Tax amount cannot be negative.");
        }

        if (request.AssignedTechnicianUserId.HasValue)
        {
            var technicianIsValid = await db.Users
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.AssignedTechnicianUserId.Value &&
                    x.BusinessId == BusinessId &&
                    x.IsActive &&
                    x.Role == "Technician");

            if (!technicianIsValid)
            {
                return BadRequest("The assigned technician is invalid or inactive.");
            }
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

        var vehicle = await db.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == request.VehicleId &&
                x.BusinessId == BusinessId);

        if (vehicle is null)
        {
            return BadRequest(
                "The selected vehicle is invalid.");
        }

        if (vehicle.CustomerId !=
            request.CustomerId)
        {
            return BadRequest(
                "The selected vehicle does not belong to the selected customer.");
        }

        if (requireActiveCustomerAndVehicle)
        {
            if (!customer.IsActive)
            {
                return BadRequest(
                    "The selected customer is inactive.");
            }

            if (!vehicle.IsActive)
            {
                return BadRequest(
                    "The selected vehicle is inactive.");
            }
        }

        var repairOrderNumber =
            request.RepairOrderNumber.Trim();

        var duplicateNumber =
            await db.RepairOrders
                .AsNoTracking()
                .AnyAsync(x =>
                    x.BusinessId == BusinessId &&
                    (!excludeRepairOrderId.HasValue ||
                     x.Id != excludeRepairOrderId.Value) &&
                    x.RepairOrderNumber ==
                        repairOrderNumber);

        if (duplicateNumber)
        {
            return BadRequest(
                "Repair order number already exists.");
        }

        return null;
    }

    private async Task Recalculate(
      RepairOrder repairOrder)
    {
        repairOrder.LaborSubtotal =
            repairOrder.LaborLines
                .Sum(x => x.LineTotal);

        repairOrder.PartsSubtotal =
            repairOrder.PartLines
                .Sum(x => x.LineTotal);

        repairOrder.Subtotal =
            repairOrder.LaborSubtotal +
            repairOrder.PartsSubtotal;

        repairOrder.TotalAmount =
            repairOrder.Subtotal +
            repairOrder.TaxAmount;

        repairOrder.AmountPaid =
            repairOrder.Payments
                .Sum(x => x.Amount);

        repairOrder.UpdatedAt =
            DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    private static DateTime ToUtc(
        DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc =>
                value,

            DateTimeKind.Local =>
                value.ToUniversalTime(),

            DateTimeKind.Unspecified =>
                DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc),

            _ =>
                DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc)
        };
    }

    private static DateTime? GetCompletedDate(
        string status,
        DateTime? requestedDate)
    {
        if (status is "Completed" or "Invoiced")
        {
            return requestedDate.HasValue
                ? ToUtc(requestedDate.Value)
                : DateTime.UtcNow;
        }

        return null;
    }

    private async Task<string?> GetTechnicianName(int? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        return await db.Users
            .AsNoTracking()
            .Where(x => x.Id == userId.Value && x.BusinessId == BusinessId)
            .Select(x => x.Name)
            .SingleOrDefaultAsync();
    }

    private static string? Clean(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

  
   
}
