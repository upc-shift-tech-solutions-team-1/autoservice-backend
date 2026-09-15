using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;
using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Task = AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;
using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Provides the main Entity Framework Core data access context, coordinating entities mapping and constraints.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    #region Properties

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for mechanics.
    /// </summary>
    public DbSet<Mechanic> Mechanics { get; set; }

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for inventory items.
    /// </summary>
    public DbSet<InventoryItem> InventoryItems { get; set; }

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for customers.
    /// </summary>
    public DbSet<Customer> Customers { get; set; }

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for vehicles.
    /// </summary>
    public DbSet<Vehicle> Vehicles { get; set; }

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for workshop work orders.
    /// </summary>
    public DbSet<WorkOrder> WorkOrders { get; set; }

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for assigned diagnostic tasks.
    /// </summary>
    public DbSet<Task> Tasks { get; set; }

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for matching parts linked to specific tasks.
    /// </summary>
    public DbSet<TaskPart> TaskParts { get; set; }

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for user platform access identities.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the database persistence tracking mapping context for tenant workshop structures.
    /// </summary>
    public DbSet<Workshop> Workshops { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Configures structural schema mapping constraints, key properties, indices, relationships, and naming configurations.
    /// </summary>
    /// <param name="builder">The active context builder instance api wrapper.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Mechanic mapping
        builder.Entity<Mechanic>().ToTable("Mechanics");
        builder.Entity<Mechanic>().HasKey(m => m.Id);
        builder.Entity<Mechanic>().Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Mechanic>().Property(m => m.FullName).IsRequired().HasMaxLength(100);
        builder.Entity<Mechanic>().Property(m => m.Specialty).IsRequired().HasMaxLength(50);
        builder.Entity<Mechanic>().Property(m => m.Email).IsRequired().HasMaxLength(100);
        builder.Entity<Mechanic>().Property(mechanic => mechanic.MaxCapacity).IsRequired();
        builder.Entity<Mechanic>().Property(mechanic => mechanic.Password).IsRequired().HasMaxLength(120);

        // InventoryItem mapping
        builder.Entity<InventoryItem>().ToTable("InventoryItems");
        builder.Entity<InventoryItem>().HasKey(i => i.Id);
        builder.Entity<InventoryItem>().Property(i => i.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<InventoryItem>().Property(i => i.Sku).IsRequired().HasMaxLength(20);
        builder.Entity<InventoryItem>().Property(i => i.Name).IsRequired().HasMaxLength(150);
        builder.Entity<InventoryItem>().Property(i => i.Category).IsRequired().HasMaxLength(50);
        builder.Entity<InventoryItem>().Property(i => i.Brand).HasMaxLength(50);
        builder.Entity<InventoryItem>().Property(i => i.QualityTier).IsRequired().HasMaxLength(20).HasDefaultValue("STANDARD");
        builder.Entity<InventoryItem>().Property(i => i.Specification).HasMaxLength(120);
        builder.Entity<InventoryItem>().Property(i => i.Presentation).HasMaxLength(100);
        builder.Entity<InventoryItem>().Property(i => i.UnitMeasure).IsRequired().HasMaxLength(30).HasDefaultValue("UNIT");
        builder.Entity<InventoryItem>().Property(i => i.PurchasePrice).IsRequired().HasColumnType("decimal(10,2)");
        builder.Entity<InventoryItem>().Property(i => i.UnitPrice).IsRequired().HasColumnType("decimal(10,2)");
        builder.Entity<InventoryItem>().Ignore(i => i.SalePrice);
        builder.Entity<InventoryItem>().Ignore(i => i.ProfitPerUnit);
        builder.Entity<InventoryItem>().Ignore(i => i.MarginPercentage);
        builder.Entity<InventoryItem>().Ignore(i => i.InventoryCostValue);
        builder.Entity<InventoryItem>().Ignore(i => i.PotentialSalesValue);
        builder.Entity<InventoryItem>().Ignore(i => i.PotentialProfitValue);
        builder.Entity<InventoryItem>().Property(i => i.Stock).IsRequired().HasDefaultValue(0);
        builder.Entity<InventoryItem>().Property(i => i.MinStock).IsRequired().HasDefaultValue(5);
        builder.Entity<InventoryItem>().Property(i => i.Image).HasColumnType("longtext");
        
        // Customer mapping
        builder.Entity<Customer>().ToTable("Customers");
        builder.Entity<Customer>().HasKey(c => c.Id);
        builder.Entity<Customer>().Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Customer>().Property(c => c.WorkshopId).IsRequired().HasMaxLength(50);
        builder.Entity<Customer>().Property(c => c.FullName).IsRequired().HasMaxLength(150);
        builder.Entity<Customer>().Property(c => c.Dni).IsRequired().HasMaxLength(20);
        builder.Entity<Customer>().Property(c => c.Email).HasMaxLength(100);
        builder.Entity<Customer>().Property(c => c.Phone).HasMaxLength(30);
        
        // Vehicle mapping
        builder.Entity<Vehicle>().ToTable("Vehicles");
        builder.Entity<Vehicle>().HasKey(v => v.Id);
        builder.Entity<Vehicle>().Property(v => v.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Vehicle>().Property(v => v.Plate).IsRequired().HasMaxLength(20);
        builder.Entity<Vehicle>().Property(v => v.Brand).HasMaxLength(50);
        builder.Entity<Vehicle>().Property(v => v.Model).HasMaxLength(50);
        builder.Entity<Vehicle>().Property(v => v.Year).HasMaxLength(10);
        builder.Entity<Vehicle>().Property(v => v.Color).HasMaxLength(30);
        builder.Entity<Vehicle>().Property(v => v.Status).IsRequired().HasMaxLength(30);
        builder.Entity<Vehicle>().Property(v => v.Image).HasColumnType("longtext");
        
        builder.Entity<Vehicle>()
            .HasOne<Customer>()
            .WithMany()
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // WorkOrder mapping
        builder.Entity<WorkOrder>().ToTable("WorkOrders");
        builder.Entity<WorkOrder>().HasKey(w => w.Id);
        builder.Entity<WorkOrder>().Property(w => w.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<WorkOrder>().Property(w => w.WorkshopId).IsRequired().HasMaxLength(50);
        builder.Entity<WorkOrder>().Property(w => w.TrackingCode).IsRequired().HasMaxLength(20);
        builder.Entity<WorkOrder>().Property(w => w.Description).HasMaxLength(500);
        builder.Entity<WorkOrder>().Property(w => w.Status).IsRequired().HasMaxLength(30);
        builder.Entity<WorkOrder>().Property(w => w.Price).HasColumnType("decimal(10,2)");
        builder.Entity<WorkOrder>().Property(w => w.TasksCompleted).HasDefaultValue(false);
        builder.Entity<WorkOrder>().Property(w => w.SparePartsChecked).HasDefaultValue(false);
        builder.Entity<WorkOrder>().Property(w => w.DiagnosisValidated).HasDefaultValue(false);
        builder.Entity<WorkOrder>().Property(w => w.CleaningDone).HasDefaultValue(false);
        builder.Entity<WorkOrder>().Property(w => w.FinalTestDone).HasDefaultValue(false);
        
        // Task mapping
        builder.Entity<Task>().ToTable("Tasks");
        builder.Entity<Task>().HasKey(t => t.Id);
        builder.Entity<Task>().Property(t => t.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Task>().Property(t => t.Description).IsRequired().HasMaxLength(250);
        builder.Entity<Task>().Property(t => t.Status).IsRequired().HasMaxLength(30);
        builder.Entity<Task>().Property(t => t.Priority).HasMaxLength(20);
        builder.Entity<Task>().Property(t => t.LaborCost).HasColumnType("decimal(10,2)");
        builder.Entity<Task>().Property(t => t.LaborPrice).HasColumnType("decimal(10,2)");
        builder.Entity<Task>().Property(t => t.MaterialsPurchaseCost).HasColumnType("decimal(10,2)");
        builder.Entity<Task>().Property(t => t.MaterialsCost).HasColumnType("decimal(10,2)");
        builder.Entity<Task>().Ignore(t => t.TotalCost);
        builder.Entity<Task>().Ignore(t => t.TotalInternalCost);
        builder.Entity<Task>().Ignore(t => t.GrossProfit);
        builder.Entity<Task>().Ignore(t => t.MarginPercentage);
        builder.Entity<Task>().Property(t => t.TechnicalDiagnosis).HasMaxLength(1000);
        builder.Entity<Task>().Property(t => t.CustomerExplanation).HasMaxLength(1000);
        builder.Entity<Task>().Property(t => t.InternalObservation).HasMaxLength(500);
        builder.Entity<Task>().Property(t => t.EvidenceRegistered).HasMaxLength(500);
        builder.Entity<Task>().Property(t => t.AdminReviewStatus).HasMaxLength(50);
        
        builder.Entity<Task>()
            .HasOne<WorkOrder>()
            .WithMany()
            .HasForeignKey(t => t.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // TaskPart mapping
        builder.Entity<TaskPart>().ToTable("TaskParts");
        builder.Entity<TaskPart>().HasKey(tp => tp.Id);
        builder.Entity<TaskPart>().Property(tp => tp.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<TaskPart>().Property(tp => tp.Name).IsRequired().HasMaxLength(150);
        builder.Entity<TaskPart>().Property(tp => tp.Brand).HasMaxLength(50);
        builder.Entity<TaskPart>().Property(tp => tp.QualityTier).IsRequired().HasMaxLength(20).HasDefaultValue("STANDARD");
        builder.Entity<TaskPart>().Property(tp => tp.PurchasePrice).HasColumnType("decimal(10,2)");
        builder.Entity<TaskPart>().Property(tp => tp.UnitPrice).HasColumnType("decimal(10,2)");
        builder.Entity<TaskPart>().Ignore(tp => tp.TotalCost);
        builder.Entity<TaskPart>().Ignore(tp => tp.TotalSale);
        builder.Entity<TaskPart>().Ignore(tp => tp.GrossProfit);
        
        builder.Entity<TaskPart>()
            .HasOne(tp => tp.Task)
            .WithMany(t => t.Parts)
            .HasForeignKey(tp => tp.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Workshop mapping
        builder.Entity<Workshop>().ToTable("Workshops");
        builder.Entity<Workshop>().HasKey(w => w.Id);
        builder.Entity<Workshop>().Property(w => w.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Workshop>().Property(w => w.Name).IsRequired().HasMaxLength(150);
        builder.Entity<Workshop>().Property(w => w.TenantId).IsRequired().HasMaxLength(50);
        
        // Auto convert naming conventions globally
        builder.UseSnakeCaseNamingConvention();
    }

    #endregion
}