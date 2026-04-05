using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class EnrolmentsController : Controller
{
    private readonly ApplicationDbContext _db;

    public EnrolmentsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? studentId, int? courseId)
    {
        var query = _db.CourseEnrolments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsQueryable();
        if (studentId.HasValue)
            query = query.Where(e => e.StudentProfileId == studentId.Value);
        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        var list = await query.OrderByDescending(e => e.EnrolDate).ToListAsync();

        var students = await _db.StudentProfiles.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        var courses = await _db.Courses.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.StudentFilter = new SelectList(students, "Id", "Name", studentId);
        ViewBag.CourseFilter = new SelectList(courses, "Id", "Name", courseId);

        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new CourseEnrolment
        {
            EnrolDate = DateTime.UtcNow.Date,
            Status = EnrolmentStatuses.Active
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseEnrolment model)
    {
        if (model.StudentProfileId <= 0 || !await _db.StudentProfiles.AsNoTracking().AnyAsync(s => s.Id == model.StudentProfileId))
            ModelState.AddModelError(nameof(model.StudentProfileId), "Please select a student.");
        if (model.CourseId <= 0 || !await _db.Courses.AsNoTracking().AnyAsync(c => c.Id == model.CourseId))
            ModelState.AddModelError(nameof(model.CourseId), "Please select a course.");

        var existing = await _db.CourseEnrolments
            .FirstOrDefaultAsync(e => e.StudentProfileId == model.StudentProfileId && e.CourseId == model.CourseId);

        if (existing != null && !EnrolmentRules.CanCreateEnrolment(existing.Status))
        {
            ModelState.AddModelError(string.Empty, "Student is already enrolled on this course.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(model.StudentProfileId, model.CourseId);
            return View(model);
        }

        if (existing != null && existing.Status == EnrolmentStatuses.Withdrawn)
        {
            existing.Status = model.Status;
            existing.EnrolDate = model.EnrolDate;
            await _db.SaveChangesAsync();
            TempData["Message"] = "Enrolment reactivated.";
            return RedirectToAction(nameof(Index));
        }

        _db.CourseEnrolments.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Enrolment created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var e = await _db.CourseEnrolments
            .Include(x => x.Student)
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        await PopulateDropdowns(e.StudentProfileId, e.CourseId);
        return View(e);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseEnrolment model)
    {
        if (id != model.Id) return BadRequest();
        if (model.StudentProfileId <= 0 || !await _db.StudentProfiles.AsNoTracking().AnyAsync(s => s.Id == model.StudentProfileId))
            ModelState.AddModelError(nameof(model.StudentProfileId), "Please select a student.");
        if (model.CourseId <= 0 || !await _db.Courses.AsNoTracking().AnyAsync(c => c.Id == model.CourseId))
            ModelState.AddModelError(nameof(model.CourseId), "Please select a course.");
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(model.StudentProfileId, model.CourseId);
            return View(model);
        }
        _db.CourseEnrolments.Update(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Enrolment updated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? studentId = null, int? courseId = null)
    {
        var students = await _db.StudentProfiles.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        var courses = await _db.Courses.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.StudentProfileId = new SelectList(students, "Id", "Name", studentId);
        ViewBag.CourseId = new SelectList(courses, "Id", "Name", courseId);
    }
}
