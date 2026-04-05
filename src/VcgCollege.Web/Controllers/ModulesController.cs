using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class ModulesController : Controller
{
    private readonly ApplicationDbContext _db;

    public ModulesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? courseId)
    {
        var query = _db.Modules.AsNoTracking().Include(m => m.Course).AsQueryable();
        if (courseId.HasValue)
            query = query.Where(m => m.CourseId == courseId.Value);
        var list = await query.OrderBy(m => m.Course!.Name).ThenBy(m => m.Title).ToListAsync();
        ViewBag.CourseFilter = courseId;
        var courses = await _db.Courses.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.CourseFilterList = new SelectList(courses, "Id", "Name", courseId);
        await PopulateCourseDropdown(courseId);
        return View(list);
    }

    public async Task<IActionResult> Create(int? courseId)
    {
        await PopulateCourseDropdown(courseId);
        return View(new Module { CourseId = courseId ?? 0, Credits = 5 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Module model)
    {
        if (model.CourseId <= 0 || !await _db.Courses.AsNoTracking().AnyAsync(c => c.Id == model.CourseId))
            ModelState.AddModelError(nameof(model.CourseId), "Please select a course.");
        if (!ModelState.IsValid)
        {
            await PopulateCourseDropdown(model.CourseId);
            return View(model);
        }
        _db.Modules.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Module created.";
        return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var module = await _db.Modules.FindAsync(id);
        if (module == null) return NotFound();
        await PopulateCourseDropdown(module.CourseId);
        return View(module);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Module model)
    {
        if (id != model.Id) return BadRequest();
        if (model.CourseId <= 0 || !await _db.Courses.AsNoTracking().AnyAsync(c => c.Id == model.CourseId))
            ModelState.AddModelError(nameof(model.CourseId), "Please select a course.");
        if (!ModelState.IsValid)
        {
            await PopulateCourseDropdown(model.CourseId);
            return View(model);
        }
        _db.Modules.Update(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Module updated.";
        return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var module = await _db.Modules.AsNoTracking()
            .Include(m => m.Course)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (module == null) return NotFound();
        return View(module);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var module = await _db.Modules.FindAsync(id);
        if (module == null) return NotFound();
        var courseId = module.CourseId;
        _db.Modules.Remove(module);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Module deleted.";
        return RedirectToAction(nameof(Index), new { courseId });
    }

    private async Task PopulateCourseDropdown(int? selectedId = null)
    {
        var courses = await _db.Courses.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.CourseId = new SelectList(courses, "Id", "Name", selectedId);
    }
}
