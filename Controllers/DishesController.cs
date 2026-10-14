using FoodManager_BIT240194.Data;
using FoodManager_BIT240194.Models;
using FoodManager_BIT240194.Services;
using FoodManager_BIT240194.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace FoodManager_BIT240194.Controllers;
public class DishesController(FoodDbContext db, UiText text) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DishFilter filter)
    {
        filter.Categories = await db.DishCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        if (filter.MinPrice < 0 || filter.MaxPrice < 0 || (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice > filter.MaxPrice))
            ModelState.AddModelError("", text["InvalidRange"]);
        if (filter.CategoryId.HasValue && !filter.Categories.Any(x => x.Id == filter.CategoryId))
            ModelState.AddModelError("", text["InvalidCategory"]);
        if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "available" && filter.Status != "unavailable")
            ModelState.AddModelError("", text["InvalidStatus"]);
        if (!new[] { "name", "price_asc", "price_desc", "time_asc" }.Contains(filter.Sort))
            ModelState.AddModelError("", text["InvalidSort"]);
        if (!ModelState.IsValid) return View(filter);
        // Compose IQueryable: SQL Server performs all search, filters and ordering.
        var query = db.Dishes.AsNoTracking().Include(x => x.DishCategory).Include(x => x.Images).AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => x.Name.Contains(filter.Search.Trim()));
        if (filter.CategoryId.HasValue) query = query.Where(x => x.DishCategoryId == filter.CategoryId);
        if (filter.Status == "available") query = query.Where(x => x.IsAvailable);
        if (filter.Status == "unavailable") query = query.Where(x => !x.IsAvailable);
        if (filter.MinPrice.HasValue) query = query.Where(x => x.Price >= filter.MinPrice);
        if (filter.MaxPrice.HasValue) query = query.Where(x => x.Price <= filter.MaxPrice);
        query = filter.Sort switch
        {
            "price_asc" => query.OrderBy(x => x.Price).ThenBy(x => x.Id),
            "price_desc" => query.OrderByDescending(x => x.Price).ThenBy(x => x.Id),
            "time_asc" => query.OrderBy(x => x.PreparationTime).ThenBy(x => x.Id),
            _ => query.OrderBy(x => x.Name).ThenBy(x => x.Id)
        };
        filter.Dishes = await query.ToListAsync();
        return View(filter);
    }
    public async Task<IActionResult> Details(int id)
    {
        var dish = await GetDish(id);
        return dish is null ? NotFound() : View(new DishDetail { Dish = dish, Image = new ImageForm { DishId = id } });
    }
    public async Task<IActionResult> Create()
    {
        var form = new DishForm();
        await LoadCategories(form);
        return View("Form", form);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DishForm form)
    {
        form.Id = 0;
        await ValidateDish(form);
        if (!ModelState.IsValid) { await LoadCategories(form); return View("Form", form); }
        var dish = new Dish_BIT240194();
        Copy(form, dish);
        if (!string.IsNullOrWhiteSpace(form.CoverImageUrl))
            dish.Images.Add(new DishImage_BIT240194 { ImageUrl = form.CoverImageUrl.Trim(), IsThumbnail = true });
        db.Dishes.Add(dish);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError("", text["SaveError"]); await LoadCategories(form); return View("Form", form); }
        TempData["Notice"] = "Saved";
        return RedirectToAction(nameof(Details), new { id = dish.Id });
    }
    public async Task<IActionResult> Edit(int id)
    {
        var dish = await db.Dishes.FindAsync(id);
        if (dish is null) return NotFound();
        var coverUrl = await db.DishImages.AsNoTracking().Where(x => x.DishId == id && x.IsThumbnail).Select(x => x.ImageUrl).SingleOrDefaultAsync();
        var form = new DishForm { Id = id, Name = dish.Name, Price = dish.Price, PreparationTime = dish.PreparationTime, IsAvailable = dish.IsAvailable, Description = dish.Description, DishCategoryId = dish.DishCategoryId, CoverImageUrl = coverUrl, CurrentCoverImageUrl = coverUrl };
        await LoadCategories(form);
        return View("Form", form);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DishForm form)
    {
        if (id != form.Id) return NotFound();
        var dish = await db.Dishes.FindAsync(id);
        if (dish is null) return NotFound();
        form.CurrentCoverImageUrl = await db.DishImages.AsNoTracking().Where(x => x.DishId == id && x.IsThumbnail).Select(x => x.ImageUrl).SingleOrDefaultAsync();
        await ValidateDish(form);
        if (!ModelState.IsValid) { await LoadCategories(form); return View("Form", form); }
        Copy(form, dish);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            if (!string.IsNullOrWhiteSpace(form.CoverImageUrl))
            {
                var cover = await db.DishImages.SingleOrDefaultAsync(x => x.DishId == id && x.IsThumbnail);
                if (cover is null)
                    db.DishImages.Add(new DishImage_BIT240194 { DishId = id, ImageUrl = form.CoverImageUrl.Trim(), IsThumbnail = true });
                else cover.ImageUrl = form.CoverImageUrl.Trim();
            }
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception error) when (error is DbUpdateException or Microsoft.Data.SqlClient.SqlException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", text["SaveError"]);
            await LoadCategories(form);
            return View("Form", form);
        }
        TempData["Notice"] = "Saved";
        return RedirectToAction(nameof(Details), new { id });
    }
    public async Task<IActionResult> Delete(int id)
    {
        var dish = await GetDish(id);
        return dish is null ? NotFound() : View(dish);
    }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dish = await db.Dishes.FindAsync(id);
        if (dish is null) return NotFound();
        db.Dishes.Remove(dish);
        try { await db.SaveChangesAsync(); TempData["Notice"] = "Deleted"; }
        catch (DbUpdateException) { TempData["Notice"] = "SaveError"; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImage([Bind(Prefix = "Image")] ImageForm image)
    {
        var dish = await GetDish(image.DishId);
        if (dish is null) return NotFound();
        image.ImageUrl = image.ImageUrl?.Trim() ?? "";
        if (!ModelState.IsValid) return View("Details", new DishDetail { Dish = dish, Image = image });
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            if (image.IsThumbnail)
                await db.DishImages.Where(x => x.DishId == image.DishId && x.IsThumbnail).ExecuteUpdateAsync(set => set.SetProperty(x => x.IsThumbnail, false));
            db.DishImages.Add(new DishImage_BIT240194 { DishId = image.DishId, ImageUrl = image.ImageUrl, IsThumbnail = image.IsThumbnail });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Notice"] = "ImageAdded";
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            TempData["Notice"] = "SaveError";
        }
        return RedirectToAction(nameof(Details), new { id = image.DishId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetThumbnail(int id)
    {
        var image = await db.DishImages.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (image is null) return NotFound();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            await db.DishImages.Where(x => x.DishId == image.DishId && x.IsThumbnail).ExecuteUpdateAsync(set => set.SetProperty(x => x.IsThumbnail, false));
            var changed = await db.DishImages.Where(x => x.Id == id).ExecuteUpdateAsync(set => set.SetProperty(x => x.IsThumbnail, true));
            if (changed == 0) { await transaction.RollbackAsync(); return NotFound(); }
            await transaction.CommitAsync();
            TempData["Notice"] = "CoverChanged";
        }
        catch (Microsoft.Data.SqlClient.SqlException) { await transaction.RollbackAsync(); TempData["Notice"] = "SaveError"; }
        return RedirectToAction(nameof(Details), new { id = image.DishId });
    }
    [HttpGet]
    public async Task<IActionResult> EditImage(int id)
    {
        var image = await db.DishImages.AsNoTracking().Include(x => x.Dish).SingleOrDefaultAsync(x => x.Id == id);
        if (image is null) return NotFound();
        return View(new ImageEditForm
        {
            Id = image.Id, ImageUrl = image.ImageUrl, DishId = image.DishId,
            DishName = image.Dish.Name, CurrentImageUrl = image.ImageUrl, IsThumbnail = image.IsThumbnail
        });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditImage(int id, [Bind("Id,ImageUrl")] ImageEditForm form)
    {
        if (id != form.Id) return NotFound();
        var image = await db.DishImages.Include(x => x.Dish).SingleOrDefaultAsync(x => x.Id == id);
        if (image is null) return NotFound();
        // The dish and cover status come from the database; only the URL can be edited here.
        form.DishId = image.DishId;
        form.DishName = image.Dish.Name;
        form.CurrentImageUrl = image.ImageUrl;
        form.IsThumbnail = image.IsThumbnail;
        if (!ModelState.IsValid) return View(form);
        image.ImageUrl = form.ImageUrl.Trim();
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        catch (DbUpdateException) { ModelState.AddModelError("", text["SaveError"]); return View(form); }
        TempData["Notice"] = "ImageUpdated";
        return RedirectToAction(nameof(Details), new { id = image.DishId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var image = await db.DishImages.FindAsync(id);
        if (image is null) return NotFound();
        var dishId = image.DishId;
        db.DishImages.Remove(image);
        try { await db.SaveChangesAsync(); TempData["Notice"] = "Deleted"; }
        catch (DbUpdateException) { TempData["Notice"] = "SaveError"; }
        return RedirectToAction(nameof(Details), new { id = dishId });
    }
    private Task<Dish_BIT240194?> GetDish(int id) => db.Dishes.AsNoTracking().Include(x => x.DishCategory).Include(x => x.Images).SingleOrDefaultAsync(x => x.Id == id);
    private async Task LoadCategories(DishForm form) => form.Categories = await db.DishCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    private async Task ValidateDish(DishForm form)
    {
        form.Name = form.Name?.Trim() ?? "";
        if (!await db.DishCategories.AnyAsync(x => x.Id == form.DishCategoryId))
            ModelState.AddModelError(nameof(form.DishCategoryId), text["InvalidCategory"]);
        if (await db.Dishes.AnyAsync(x => x.Id != form.Id && x.DishCategoryId == form.DishCategoryId && x.Name == form.Name))
            ModelState.AddModelError(nameof(form.Name), text["DuplicateDish"]);
    }
    private static void Copy(DishForm form, Dish_BIT240194 dish)
    {
        dish.Name = form.Name; dish.Price = form.Price!.Value; dish.PreparationTime = form.PreparationTime!.Value;
        dish.IsAvailable = form.IsAvailable; dish.Description = form.Description?.Trim(); dish.DishCategoryId = form.DishCategoryId!.Value;
    }
}
