using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class AttendanceAdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AttendanceAdminController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? courseId)
    {
        var enrolQuery = _db.CourseEnrolments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsQueryable();
        if (courseId.HasValue)
            enrolQuery = enrolQuery.Where(e => e.CourseId == courseId.Value);

        var list = await enrolQuery.OrderBy(e => e.Course!.Name).ThenBy(e => e.Student!.Name).ToListAsync();
        var courses = await _db.Courses.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.CourseFilter = new SelectList(courses, "Id", "Name", courseId);
        return View(list);
    }

    public async Task<IActionResult> ForEnrolment(int id)
    {
        var enrol = await _db.CourseEnrolments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.AttendanceRecords)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (enrol == null) return NotFound();
        return View(enrol);
    }

    public async Task<IActionResult> Create(int enrolmentId)
    {
        var enrol = await _db.CourseEnrolments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == enrolmentId);
        if (enrol == null) return NotFound();

        var nextWeek = await _db.AttendanceRecords
            .Where(a => a.CourseEnrolmentId == enrolmentId)
            .Select(a => (int?)a.WeekNumber)
            .MaxAsync() ?? 0;

        return View(new AttendanceRecord
        {
            CourseEnrolmentId = enrolmentId,
            WeekNumber = nextWeek + 1,
            SessionDate = DateTime.UtcNow.Date,
            Present = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AttendanceRecord model)
    {
        if (await _db.AttendanceRecords.AnyAsync(a =>
                a.CourseEnrolmentId == model.CourseEnrolmentId && a.WeekNumber == model.WeekNumber))
        {
            ModelState.AddModelError(nameof(model.WeekNumber), "Week number already recorded for this enrolment.");
        }

        if (!ModelState.IsValid)
            return View(model);

        _db.AttendanceRecords.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Attendance saved.";
        return RedirectToAction(nameof(ForEnrolment), new { id = model.CourseEnrolmentId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var row = await _db.AttendanceRecords
            .Include(a => a.CourseEnrolment).ThenInclude(e => e.Student)
            .Include(a => a.CourseEnrolment).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (row == null) return NotFound();
        return View(row);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AttendanceRecord model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
            return View(model);

        _db.AttendanceRecords.Update(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Attendance updated.";
        return RedirectToAction(nameof(ForEnrolment), new { id = model.CourseEnrolmentId });
    }
}
