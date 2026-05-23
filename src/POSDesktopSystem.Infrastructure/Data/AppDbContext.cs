using Microsoft.EntityFrameworkCore;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Enums;
using System;

namespace POSDesktopSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Barcode).IsUnique();
        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
            .Property(p => p.CostPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => p.IsActive);   // global soft delete filter

        // Invoice
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber).IsUnique();
        modelBuilder.Entity<Invoice>()
            .Property(i => i.SubTotal).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.DiscountAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxRate).HasPrecision(5, 4);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.Status).HasConversion<string>();

        // InvoiceItem
        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.LineTotal).HasPrecision(18, 2);

        // Payment
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Invoice)
            .WithOne(i => i.Payment)
            .HasForeignKey<Payment>(p => p.InvoiceId);
        modelBuilder.Entity<Payment>()
            .Property(p => p.AmountPaid).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>()
            .Property(p => p.Change).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>()
            .Property(p => p.Method).HasConversion<string>();

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .Property(u => u.Role).HasConversion<string>();

        // Seed: manager account
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "manager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                Role = UserRole.Manager,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 2,
                Username = "cashier",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cashier@123"),
                Role = UserRole.Cashier,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed: Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Barcode = "7501031311309", Name = "Coca Cola 600ml", Category = "Beverages", Price = 18.50m, CostPrice = 12.00m, StockQuantity = 50, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Barcode = "7501031311310", Name = "Pepsi 600ml", Category = "Beverages", Price = 17.00m, CostPrice = 11.00m, StockQuantity = 40, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Barcode = "1234567890123", Name = "Lays Classic", Category = "Snacks", Price = 15.00m, CostPrice = 8.50m, StockQuantity = 100, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, Barcode = "9876543210987", Name = "Milk 1L", Category = "Dairy", Price = 25.00m, CostPrice = 20.00m, StockQuantity = 30, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 5, Barcode = "1111111111111", Name = "White Bread", Category = "Bakery", Price = 35.00m, CostPrice = 28.00m, StockQuantity = 20, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
    }
}
