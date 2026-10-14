using FoodManager_BIT240194.Models;
using Microsoft.EntityFrameworkCore;
namespace FoodManager_BIT240194.Data;
public class FoodDbContext(DbContextOptions<FoodDbContext> options) : DbContext(options)
{
    public DbSet<Dish_BIT240194> Dishes => Set<Dish_BIT240194>();
    public DbSet<DishCategory_BIT240194> DishCategories => Set<DishCategory_BIT240194>();
    public DbSet<DishImage_BIT240194> DishImages => Set<DishImage_BIT240194>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var categories = modelBuilder.Entity<DishCategory_BIT240194>();
        categories.ToTable("BIT240194_DishCategories");
        categories.Property(x => x.Name).HasMaxLength(120).IsRequired();
        categories.Property(x => x.Description).HasMaxLength(2000);
        var dishes = modelBuilder.Entity<Dish_BIT240194>();
        dishes.ToTable("BIT240194_Dishes", table =>
        {
            table.HasCheckConstraint("CK_BIT240194_Price", "[BIT240194_Price] > 0");
            table.HasCheckConstraint("CK_BIT240194_PreparationTime", "[BIT240194_PreparationTime] > 0");
        });
        dishes.Property(x => x.Name).HasMaxLength(120).IsRequired();
        dishes.Property(x => x.Description).HasMaxLength(2000);
        dishes.Property(x => x.Price).HasPrecision(18, 2);
        dishes.HasIndex(x => new { x.DishCategoryId, x.Name }).IsUnique();
        dishes.HasOne(x => x.DishCategory).WithMany(x => x.Dishes).HasForeignKey(x => x.DishCategoryId).OnDelete(DeleteBehavior.Restrict);
        var images = modelBuilder.Entity<DishImage_BIT240194>();
        images.ToTable("BIT240194_DishImages");
        images.Property(x => x.ImageUrl).HasMaxLength(2048).IsRequired();
        images.HasOne(x => x.Dish).WithMany(x => x.Images).HasForeignKey(x => x.DishId).OnDelete(DeleteBehavior.Cascade);
        images.HasIndex(x => x.DishId).IsUnique().HasFilter("[BIT240194_IsThumbnail] = 1");
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
                property.SetColumnName("BIT240194_" + property.Name);
    }
}
