namespace FoodManager_BIT240194.Models;
public class DishCategory_BIT240194
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public ICollection<Dish_BIT240194> Dishes { get; set; } = new List<Dish_BIT240194>();
}
public class Dish_BIT240194
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int PreparationTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? Description { get; set; }
    public int DishCategoryId { get; set; }
    public DishCategory_BIT240194 DishCategory { get; set; } = null!;
    public ICollection<DishImage_BIT240194> Images { get; set; } = new List<DishImage_BIT240194>();
}
public class DishImage_BIT240194
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = "";
    public bool IsThumbnail { get; set; }
    public int DishId { get; set; }
    public Dish_BIT240194 Dish { get; set; } = null!;
}
