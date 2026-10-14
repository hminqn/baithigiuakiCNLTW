using FoodManager_BIT240194.Data;
using FoodManager_BIT240194.Models;
using FoodManager_BIT240194.Services;
using FoodManager_BIT240194.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoodManager_BIT240194.Controllers;
public class CategoriesController(FoodDbContext db, UiText text) : Controller
{
    public async Task<IActionResult> Index() => View(await db.DishCategories.AsNoTracking().Include(x => x.Dishes).OrderBy(x => x.Name).ToListAsync());
    public IActionResult Create() => View("Form", new CategoryForm());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryForm form)
    {
        form.Id = 0;
        if (!ModelState.IsValid) return View("Form", form);
        db.DishCategories.Add(new DishCategory_BIT240194 { Name = form.Name.Trim(), Description = form.Description?.Trim() });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError("", text["SaveError"]); return View("Form", form); }
        TempData["Notice"] = "Saved";
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(int id)
    {
        var category = await db.DishCategories.FindAsync(id);
        return category is null ? NotFound() : View("Form", new CategoryForm { Id = id, Name = category.Name, Description = category.Description });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryForm form)
    {
        if (id != form.Id) return NotFound();
        var category = await db.DishCategories.FindAsync(id);
        if (category is null) return NotFound();
        if (!ModelState.IsValid) return View("Form", form);
        category.Name = form.Name.Trim(); category.Description = form.Description?.Trim();
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError("", text["SaveError"]); return View("Form", form); }
        TempData["Notice"] = "Saved";
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.DishCategories.AsNoTracking().Include(x => x.Dishes).SingleOrDefaultAsync(x => x.Id == id);
        return category is null ? NotFound() : View(category);
    }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await db.DishCategories.Include(x => x.Dishes).SingleOrDefaultAsync(x => x.Id == id);
        if (category is null) return NotFound();
        if (category.Dishes.Count != 0) { ModelState.AddModelError("", text["CategoryInUse"]); return View(category); }
        db.DishCategories.Remove(category);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError("", text["CategoryInUse"]); return View(category); }
        TempData["Notice"] = "Deleted";
        return RedirectToAction(nameof(Index));
    }
}
