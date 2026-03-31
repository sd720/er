using ItemProcessorApp.Data;
using ItemProcessorApp.Models;
using ItemProcessorApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ItemProcessorApp.Controllers;

public class ProcessController : Controller
{
    private readonly AppDbContext _db;

    public ProcessController(AppDbContext db)
    {
        _db = db;
    }

    // GET /Process/Create
    public async Task<IActionResult> Create()
    {
        var items = await _db.Items.OrderBy(i => i.Name).ToListAsync();
        ViewBag.Items = new SelectList(items, "Id", "Name");
        return View(new ProcessItemViewModel());
    }

    // POST /Process/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProcessItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var itemList = await _db.Items.OrderBy(i => i.Name).ToListAsync();
            ViewBag.Items = new SelectList(itemList, "Id", "Name");
            return View(model);
        }

        var userId = HttpContext.Session.GetInt32("UserId");

        // Create the parent ProcessedItem
        var parent = new ProcessedItem
        {
            ItemId = model.ParentItemId,
            ParentId = null,
            OutputWeight = model.ParentOutputWeight,
            Notes = model.ParentNotes,
            ProcessedBy = userId,
            ProcessedAt = DateTime.Now
        };
        _db.ProcessedItems.Add(parent);
        await _db.SaveChangesAsync();

        // Create each child ProcessedItem
        foreach (var child in model.Children.Where(c => c.ItemId > 0 && c.OutputWeight > 0))
        {
            var childItem = new ProcessedItem
            {
                ItemId = child.ItemId,
                ParentId = parent.Id,
                OutputWeight = child.OutputWeight,
                Notes = child.Notes,
                ProcessedBy = userId,
                ProcessedAt = DateTime.Now
            };
            _db.ProcessedItems.Add(childItem);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Item processed successfully with child outputs!";
        return RedirectToAction(nameof(Tree));
    }

    // GET /Process/Tree  -- Recursive tree view of all processed items
    public async Task<IActionResult> Tree()
    {
        // Load all root processed items (ParentId == null) with full child tree
        var roots = await _db.ProcessedItems
            .Include(p => p.Item)
            .Include(p => p.Processor)
            .Where(p => p.ParentId == null)
            .OrderByDescending(p => p.ProcessedAt)
            .ToListAsync();

        // Load ALL processed items once (for building child collections in memory)
        var all = await _db.ProcessedItems
            .Include(p => p.Item)
            .ToListAsync();

        // Build the in-memory tree
        foreach (var node in all)
        {
            node.Children = all.Where(c => c.ParentId == node.Id).ToList();
        }

        return View(roots);
    }

    // POST /Process/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.ProcessedItems
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (item == null) return NotFound();

        // Recursively delete children first
        await DeleteRecursive(id);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Processed item and all its outputs deleted.";
        return RedirectToAction(nameof(Tree));
    }

    private async Task DeleteRecursive(int parentId)
    {
        var children = await _db.ProcessedItems
            .Where(p => p.ParentId == parentId)
            .ToListAsync();

        foreach (var child in children)
            await DeleteRecursive(child.Id);

        var parent = await _db.ProcessedItems.FindAsync(parentId);
        if (parent != null)
            _db.ProcessedItems.Remove(parent);
    }
}
