using Microsoft.EntityFrameworkCore;
using Thesis.Assets.Models;

namespace Thesis.Assets;

/// <summary>
/// Контекст базы данных
/// </summary>
public sealed class DatabaseContext : DbContext
{
    #region Tables

    /// <summary>
    /// Категории активов
    /// </summary>
    public DbSet<Asset> Assets { get; set; } = null!;
    
    /// <summary>
    /// Категории категорий активов
    /// </summary>
    public DbSet<Category> Categories { get; set; } = null!;

    #endregion
    
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public DatabaseContext() { }
    
    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    /// <param name="options">Параметры</param>
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) 
    {
        if (Database.GetPendingMigrations().Any())
            Database.Migrate();
    }

    /// <inheritdoc cref="DbContext.OnModelCreating(ModelBuilder)"/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.AreaId).IsRequired();
            
            entity.HasMany(e => e.Assets).WithOne(e => e.Category).HasForeignKey(e => e.CategoryId);
        });
        
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.AreaId).IsRequired();
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.Parents).IsRequired();
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            
            entity.HasOne(e => e.Category).WithMany(e => e.Assets).HasForeignKey(e => e.CategoryId);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}
