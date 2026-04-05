using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _db;

    public CoursesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.Courses.AsNoTracking()
            .Include(c => c.Branch)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateBranchDropdown();
        return View(new Course { StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(6) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course model)
    {
        if (model.BranchId <= 0 || !await _db.Branches.AsNoTracking().AnyAsync(b => b.Id == model.BranchId))
            ModelState.AddModelError(nameof(model.BranchId), "Please select a valid branch.");
        if (model.EndDate < model.StartDate)
            ModelState.AddModelError(nameof(model.EndDate), "End date must be on or after start date.");
        if (!ModelState.IsValid)
        {
            await PopulateBranchDropdown(model.BranchId);
            return View(model);
        }
        _db.Courses.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Course created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();
        await PopulateBranchDropdown(course.BranchId);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course model)
    {
        if (id != model.Id) return BadRequest();
        if (model.BranchId <= 0 || !await _db.Branches.AsNoTracking().AnyAsync(b => b.Id == model.BranchId))
            ModelState.AddModelError(nameof(model.BranchId), "Please select a valid branch.");
        if (model.EndDate < model.StartDate)
            ModelState.AddModelError(nameof(model.EndDate), "End date must be on or after start date.");
        if (!ModelState.IsValid)
        {
            await PopulateBranchDropdown(model.BranchId);
            return View(model);
        }
        _db.Courses.Update(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Course updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Modules)
            .Include(c => c.FacultyAssignments).ThenInclude(a => a.Faculty)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();
        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Course deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateBranchDropdown(int? selectedId = null)
    {
        var branches = await _db.Branches.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        ViewBag.BranchId = new SelectList(branches, "Id", "Name", selectedId);
    }
}
