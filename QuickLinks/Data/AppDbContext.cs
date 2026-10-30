using Microsoft.EntityFrameworkCore;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryApplication> CategoryApplications => Set<CategoryApplication>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<ApplicationGroup> ApplicationGroups => Set<ApplicationGroup>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<ApplicationLocation> ApplicationLocations => Set<ApplicationLocation>();
    public DbSet<CarouselImage> CarouselImages => Set<CarouselImage>();
    public DbSet<Advice> Advices => Set<Advice>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Sella LastUpdated en UTC en toda entidad auditada que se inserte o modifique.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditedEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.LastUpdated = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
