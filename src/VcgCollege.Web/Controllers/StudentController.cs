using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Student)]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IUserContextAccessor _userContext;

    public StudentController(ApplicationDbContext db, IUserContextAccessor userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<IActionResult> Index()
    {
        var sid = await _userContext.GetCurrentStudentProfileIdAsync();
        if (sid == null) return Forbid();

        var profile = await _db.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sid.Value);
        var enrolCount = await _db.CourseEnrolments.CountAsync(e => e.StudentProfileId == sid && e.Status == EnrolmentStatuses.Active);
        ViewBag.EnrolmentCount = enrolCount;
        return View(profile);
    }

    public async Task<IActionResult> Profile()
    {
        var sid = await _userContext.GetCurrentStudentProfileIdAsync();
        if (sid == null) return Forbid();
        var profile = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.Id == sid.Value);
        if (profile == null) return NotFound();
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(StudentProfile model)
    {
        var sid = await _userContext.GetCurrentStudentProfileIdAsync();
        if (sid == null) return Forbid();
        if (model.Id != sid.Value) return Forbid();

        var profile = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.Id == sid.Value);
        if (profile == null) return NotFound();

        profile.Phone = model.Phone;
        profile.Address = model.Address;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> Assignments()
    {
        var sid = await _userContext.GetCurrentStudentProfileIdAsync();
        if (sid == null) return Forbid();

        var courseIds = await _db.CourseEnrolments.AsNoTracking()
            .Where(e => e.StudentProfileId == sid && e.Status == EnrolmentStatuses.Active)
            .Select(e => e.CourseId)
            .ToListAsync();

        var data = await _db.Assignments.AsNoTracking()
            .Where(a => courseIds.Contains(a.CourseId))
            .Include(a => a.Course)
            .OrderBy(a => a.Course!.Name).ThenBy(a => a.DueDate)
            .ToListAsync();

        var assignmentIds = data.Select(a => a.Id).ToList();
        var results = await _db.AssignmentResults.AsNoTracking()
            .Where(r => r.StudentProfileId == sid && assignmentIds.Contains(r.AssignmentId))
            .ToListAsync();
        var resultMap = results.ToDictionary(r => r.AssignmentId);

        ViewBag.Results = resultMap;
        return View(data);
    }

    public async Task<IActionResult> Exams()
    {
        var sid = await _userContext.GetCurrentStudentProfileIdAsync();
        if (sid == null) return Forbid();

        var courseIds = await _db.CourseEnrolments.AsNoTracking()
            .Where(e => e.StudentProfileId == sid && e.Status == EnrolmentStatuses.Active)
            .Select(e => e.CourseId)
            .ToListAsync();

        var exams = await _db.Exams.AsNoTracking()
            .Where(e => courseIds.Contains(e.CourseId))
            .Include(e => e.Course)
            .OrderBy(e => e.Course!.Name).ThenBy(e => e.Date)
            .ToListAsync();

        var map = await StudentExamViewQueries.GetVisibleExamResultsMapAsync(_db, sid.Value);

        ViewBag.Results = map;
        return View(exams);
    }

    public async Task<IActionResult> Attendance()
    {
        var sid = await _userContext.GetCurrentStudentProfileIdAsync();
        if (sid == null) return Forbid();

        var enrolments = await _db.CourseEnrolments.AsNoTracking()
            .Where(e => e.StudentProfileId == sid && e.Status == EnrolmentStatuses.Active)
            .Include(e => e.Course)
            .Include(e => e.AttendanceRecords)
            .OrderBy(e => e.Course!.Name)
            .ToListAsync();

        return View(enrolments);
    }
}
