using FoodManager_BIT240194.Models;
using Microsoft.EntityFrameworkCore;
namespace FoodManager_BIT240194.Data;
public static class SeedData
{
    public static async Task InitializeAsync(FoodDbContext db)
    {
        if (await db.DishCategories.AnyAsync()) return;
        var main = new DishCategory_BIT240194 { Name = "Main courses", Description = "Freshly prepared Vietnamese favourites." };
        var starters = new DishCategory_BIT240194 { Name = "Starters", Description = "Small plates to begin your meal." };
        var desserts = new DishCategory_BIT240194 { Name = "Desserts", Description = "A sweet finish." };
        db.DishCategories.AddRange(main, starters, desserts);
        db.Dishes.AddRange(
            Dish("Phở bò", 65000, 25, true, "Beef noodle soup with fresh herbs.", main, "/images/pho.svg"),
            Dish("Cơm gà", 55000, 20, true, "Chicken rice with ginger and seasonal vegetables.", main, "/images/rice.svg"),
            Dish("Gỏi cuốn", 35000, 10, true, "Fresh spring rolls with peanut dipping sauce.", starters, "/images/rolls.svg"),
            Dish("Chả giò", 40000, 15, false, "Crispy spring rolls. Temporarily unavailable.", starters, "/images/rolls.svg"),
            Dish("Chè đậu xanh", 25000, 12, true, "Mung bean dessert with coconut milk.", desserts, "/images/dessert.svg"));
        await db.SaveChangesAsync();
    }
    private static Dish_BIT240194 Dish(string name, decimal price, int time, bool available, string description, DishCategory_BIT240194 category, string image) =>
        new() { Name = name, Price = price, PreparationTime = time, IsAvailable = available, Description = description, DishCategory = category,
            Images = new List<DishImage_BIT240194> { new() { ImageUrl = image, IsThumbnail = true }, new() { ImageUrl = "/images/table.svg", IsThumbnail = false } } };
}
