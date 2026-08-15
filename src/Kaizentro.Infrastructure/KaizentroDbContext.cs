using Kaizentro.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kaizentro.Infrastructure;

public sealed class KaizentroDbContext(DbContextOptions<KaizentroDbContext> options) : DbContext(options)
{
    public DbSet<PlantRecord> Plants => Set<PlantRecord>();
    public DbSet<DepartmentRecord> Departments => Set<DepartmentRecord>();
    public DbSet<WorkCenterRecord> WorkCenters => Set<WorkCenterRecord>();
    public DbSet<MaterialRecord> Materials => Set<MaterialRecord>();
    public DbSet<RoutingOperationRecord> RoutingOperations => Set<RoutingOperationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlantRecord>().HasKey(x => x.Id);
        modelBuilder.Entity<DepartmentRecord>().HasKey(x => x.Id);
        modelBuilder.Entity<WorkCenterRecord>().HasKey(x => x.Id);
        modelBuilder.Entity<MaterialRecord>().HasKey(x => x.Id);
        modelBuilder.Entity<RoutingOperationRecord>().HasKey(x => x.Id);
    }
}

public sealed class PlantRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}

public sealed class DepartmentRecord
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Manager { get; set; }
}

public sealed class WorkCenterRecord
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal AvailableMinutesPerShift { get; set; }
    public decimal EfficiencyPercent { get; set; }
}

public sealed class MaterialRecord
{
    public Guid Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProductFamily { get; set; } = string.Empty;
}

public sealed class RoutingOperationRecord
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public Guid MaterialId { get; set; }
    public Guid WorkCenterId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public decimal CycleTimeSeconds { get; set; }
    public decimal SetupTimeMinutes { get; set; }
    public decimal QueueTimeMinutes { get; set; }
    public decimal MoveTimeMinutes { get; set; }
    public int Operators { get; set; }
    public decimal YieldPercent { get; set; }
}
