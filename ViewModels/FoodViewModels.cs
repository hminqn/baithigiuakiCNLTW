using System.ComponentModel.DataAnnotations;
using FoodManager_BIT240194.Models;
using FoodManager_BIT240194.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace FoodManager_BIT240194.ViewModels;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class LocalRuleAttribute(string rule) : ValidationAttribute
{
    public override object TypeId => (GetType(), rule);
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var valid = rule switch
        {
            "Required" => value is string text && !string.IsNullOrWhiteSpace(text),
            "NameLength" => value is not string name || name.Length <= 120,
            "DescriptionLength" => value is not string description || description.Length <= 2000,
            "PositivePrice" => value is decimal price && price > 0 && price <= 999999999 && decimal.Round(price, 2) == price,
            "PositiveTime" => value is int time && time > 0,
            "InvalidCategory" => value is int id && id > 0,
            "InvalidUrl" => value is string url && IsImageUrl(url),
            "OptionalImageUrl" => value is null || (value is string optionalUrl && (string.IsNullOrWhiteSpace(optionalUrl) || IsImageUrl(optionalUrl))),
            _ => true
        };
        return valid ? ValidationResult.Success : new ValidationResult(UiText.Message(rule == "OptionalImageUrl" ? "InvalidUrl" : rule));
    }
    private static bool IsImageUrl(string value)
    {
        var url = value.Trim();
        return url.Length <= 2048 &&
            ((url.StartsWith("/images/", StringComparison.Ordinal) && !url.Contains("..") && !url.Contains('\\'))
            || (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https")));
    }
}
public class DishForm
{
    public int Id { get; set; }
    [LocalRule("Required"), LocalRule("NameLength")] public string Name { get; set; } = "";
    [LocalRule("PositivePrice")] public decimal? Price { get; set; }
    [LocalRule("PositiveTime")] public int? PreparationTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    [LocalRule("DescriptionLength")] public string? Description { get; set; }
    [LocalRule("InvalidCategory")] public int? DishCategoryId { get; set; }
    [LocalRule("OptionalImageUrl")] public string? CoverImageUrl { get; set; }
    [BindNever] public string? CurrentCoverImageUrl { get; set; }
    public List<DishCategory_BIT240194> Categories { get; set; } = [];
}
public class CategoryForm
{
    public int Id { get; set; }
    [LocalRule("Required"), LocalRule("NameLength")] public string Name { get; set; } = "";
    [LocalRule("DescriptionLength")] public string? Description { get; set; }
}
public class ImageForm
{
    public int DishId { get; set; }
    [LocalRule("Required"), LocalRule("InvalidUrl")] public string ImageUrl { get; set; } = "";
    public bool IsThumbnail { get; set; }
}
public class ImageEditForm
{
    public int Id { get; set; }
    [LocalRule("Required"), LocalRule("InvalidUrl")] public string ImageUrl { get; set; } = "";
    public int DishId { get; set; }
    public string DishName { get; set; } = "";
    public string CurrentImageUrl { get; set; } = "";
    public bool IsThumbnail { get; set; }
}
public class DishDetail
{
    public Dish_BIT240194 Dish { get; set; } = null!;
    public ImageForm Image { get; set; } = new();
}
public class DishFilter
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Sort { get; set; } = "name";
    public List<Dish_BIT240194> Dishes { get; set; } = [];
    public List<DishCategory_BIT240194> Categories { get; set; } = [];
}
