using ItemProcessorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ItemProcessorApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ProcessedItem> ProcessedItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Item decimal precision
        modelBuilder.Entity<Item>()
            .Property(i => i.Weight)
            .HasColumnType("decimal(18,4)");

        // ProcessedItem decimal precision
        modelBuilder.Entity<ProcessedItem>()
            .Property(p => p.OutputWeight)
            .HasColumnType("decimal(18,4)");

        // Self-referencing relationship (parent-child tree)
        modelBuilder.Entity<ProcessedItem>()
            .HasOne(p => p.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Item -> ProcessedItems: restrict delete
        modelBuilder.Entity<ProcessedItem>()
            .HasOne(p => p.Item)
            .WithMany(i => i.ProcessedItems)
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // User -> Items created by
        modelBuilder.Entity<Item>()
            .HasOne(i => i.Creator)
            .WithMany(u => u.CreatedItems)
            .HasForeignKey(i => i.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // User -> ProcessedItems processed by
        modelBuilder.Entity<ProcessedItem>()
            .HasOne(p => p.Processor)
            .WithMany(u => u.ProcessedItems)
            .HasForeignKey(p => p.ProcessedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
