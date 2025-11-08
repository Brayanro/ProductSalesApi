using Microsoft.EntityFrameworkCore;
using ProductSalesApi.Entities;

namespace ProductSalesApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<SaleItem>().Property(s => s.UnitPrice).HasPrecision(18, 2);
        
        modelBuilder.Entity<Sale>()
            .Property(s => s.Date)
            .HasConversion(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v)  
            )
            .HasColumnType("date");


        base.OnModelCreating(modelBuilder);
    }
}