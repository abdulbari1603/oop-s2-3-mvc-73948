using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class BranchesController : Controller
{
    private readonly ApplicationDbContext _db;

    public BranchesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.Branches.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        return View(list);
    }

    public IActionResult Create() => View(new Branch());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Branch model)
    {
        if (!ModelState.IsValid)
            return View(model);
        _db.Branches.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Branch created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await _db.Branches.FindAsync(id);
        if (branch == null) return NotFound();
        return View(branch);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Branch model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
            return View(model);
        _db.Branches.Update(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Branch updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var branch = await _db.Branches.AsNoTracking()
            .Include(b => b.Courses)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null) return NotFound();
        return View(branch);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null) return NotFound();
        return View(branch);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var branch = await _db.Branches.FindAsync(id);
        if (branch == null) return NotFound();
        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Branch deleted.";
        return RedirectToAction(nameof(Index));
    }
}
