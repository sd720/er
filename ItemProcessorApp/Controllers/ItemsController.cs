using ItemProcessorApp.Data;
using ItemProcessorApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemProcessorApp.Controllers;

public class ItemsController : Controller
{
    private readonly AppDbContext _db;

    public ItemsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /Items  (List + Search)
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;
        var items = _db.Items.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            items = items.Where(i =>
                i.Name.ToLower().Contains(search) ||
                (i.Description != null && i.Description.ToLower().Contains(search)));
        }

        return View(await items.OrderByDescending(i => i.CreatedAt).ToListAsync());
    }

    // GET /Items/Create
    public IActionResult Create() => View();

    // POST /Items/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Item item)
    {
        if (!ModelState.IsValid)
            return View(item);

        item.CreatedBy = HttpContext.Session.GetInt32("UserId");
        item.CreatedAt = DateTime.Now;
        item.UpdatedAt = DateTime.Now;

        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Item \"{item.Name}\" added successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET /Items/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST /Items/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Item item)
    {
        if (id != item.Id) return BadRequest();
        if (!ModelState.IsValid) return View(item);

        var existing = await _db.Items.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = item.Name;
        existing.Weight = item.Weight;
        existing.Description = item.Description;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Item \"{item.Name}\" updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET /Items/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Items
            .Include(i => i.Creator)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST /Items/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();

        // Check if item has been processed
        var isUsed = await _db.ProcessedItems.AnyAsync(p => p.ItemId == id);
        if (isUsed)
        {
            TempData["Error"] = "Cannot delete this item because it has been used in processing. Please delete the processed records first.";
            return RedirectToAction(nameof(Index));
        }

        _db.Items.Remove(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Item \"{item.Name}\" deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
