using CloudMartAutoShop.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudMartAutoShop.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();
    public DbSet<RepairOrderLabor> RepairOrderLabors => Set<RepairOrderLabor>();
    public DbSet<RepairOrderPart> RepairOrderParts => Set<RepairOrderPart>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PartSupplier> PartSuppliers => Set<PartSupplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Business
        modelBuilder.Entity<Business>()
            .Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        // User
        modelBuilder.Entity<User>()
            .HasIndex(x => new { x.BusinessId, x.Email })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(x => x.Business)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        // Customer
        modelBuilder.Entity<Customer>()
            .HasOne(x => x.Business)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasIndex(x => new { x.BusinessId, x.Name });

        // Vehicle
        modelBuilder.Entity<Vehicle>()
            .HasOne(x => x.Business)
            .WithMany()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehicle>()
            .HasOne(x => x.Customer)
            .WithMany(x => x.Vehicles)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(x => new { x.BusinessId, x.Vin });

        // Repair Order
        modelBuilder.Entity<RepairOrder>()
            .HasIndex(x => new { x.BusinessId, x.RepairOrderNumber })
            .IsUnique();

        modelBuilder.Entity<RepairOrder>()
            .HasOne(x => x.Business)
            .WithMany()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairOrder>()
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairOrder>()
            .HasOne(x => x.Vehicle)
            .WithMany(x => x.RepairOrders)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Repair Order money
        modelBuilder.Entity<RepairOrder>()
            .Property(x => x.LaborSubtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrder>()
            .Property(x => x.PartsSubtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrder>()
            .Property(x => x.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrder>()
            .Property(x => x.TaxAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrder>()
            .Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrder>()
            .Property(x => x.AmountPaid)
            .HasPrecision(18, 2);

        // Labor
        modelBuilder.Entity<RepairOrderLabor>()
            .HasOne(x => x.RepairOrder)
            .WithMany(x => x.LaborLines)
            .HasForeignKey(x => x.RepairOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RepairOrderLabor>()
            .Property(x => x.Hours)
            .HasPrecision(10, 2);

        modelBuilder.Entity<RepairOrderLabor>()
            .Property(x => x.HourlyRate)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrderLabor>()
            .Property(x => x.LineTotal)
            .HasPrecision(18, 2);

        // Repair Order Parts
        modelBuilder.Entity<RepairOrderPart>()
            .HasOne(x => x.RepairOrder)
            .WithMany(x => x.PartLines)
            .HasForeignKey(x => x.RepairOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RepairOrderPart>()
            .Property(x => x.Quantity)
            .HasPrecision(18, 3);

        modelBuilder.Entity<RepairOrderPart>()
            .Property(x => x.UnitCost)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrderPart>()
            .Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrderPart>()
            .Property(x => x.LineTotal)
            .HasPrecision(18, 2);

        // Repair Order Part -> Supplier actually used
        modelBuilder.Entity<RepairOrderPart>()
            .HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairOrderPart>()
            .HasIndex(x => new { x.BusinessId, x.SupplierId });

        // Repair Order Part -> reusable Part catalog
        modelBuilder.Entity<RepairOrderPart>()
            .HasOne(x => x.Part)
            .WithMany()
            .HasForeignKey(x => x.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairOrderPart>()
            .HasIndex(x => new { x.BusinessId, x.PartId });

        // Payments
        modelBuilder.Entity<Payment>()
            .HasOne(x => x.RepairOrder)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.RepairOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        // Supplier
        modelBuilder.Entity<Supplier>()
            .HasOne(x => x.Business)
            .WithMany(x => x.Suppliers)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Supplier>()
            .HasIndex(x => new { x.BusinessId, x.Name });

        // Part
        modelBuilder.Entity<Part>()
            .HasOne(x => x.Business)
            .WithMany()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Part>()
            .HasOne(x => x.PreferredSupplier)
            .WithMany()
            .HasForeignKey(x => x.PreferredSupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Part>()
            .HasIndex(x => new { x.BusinessId, x.PartNumber });

        modelBuilder.Entity<Part>()
            .Property(x => x.DefaultUnitCost)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Part>()
            .Property(x => x.DefaultUnitPrice)
            .HasPrecision(18, 2);

        // Part <-> Supplier
        modelBuilder.Entity<PartSupplier>()
            .HasOne(x => x.Part)
            .WithMany(x => x.Suppliers)
            .HasForeignKey(x => x.PartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PartSupplier>()
            .HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PartSupplier>()
            .HasIndex(x => new
            {
                x.BusinessId,
                x.PartId,
                x.SupplierId
            })
            .IsUnique();

        modelBuilder.Entity<PartSupplier>()
            .Property(x => x.LastCost)
            .HasPrecision(18, 2);
    }
}