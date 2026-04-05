using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class AssignmentsAdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AssignmentsAdminController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var courses = await _db.Courses.AsNoTracking()
            .Include(c => c.Branch)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(courses);
    }

    public async Task<IActionResult> Course(int id)
    {
        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    public IActionResult Create(int courseId) =>
        View(new Assignment { CourseId = courseId, DueDate = DateTime.UtcNow.Date.AddDays(7), MaxScore = 100 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Assignment model)
    {
        if (model.MaxScore <= 0)
            ModelState.AddModelError(nameof(model.MaxScore), "Max score must be positive.");
        if (!ModelState.IsValid)
            return View(model);
        _db.Assignments.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Assignment created.";
        return RedirectToAction(nameof(Course), new { id = model.CourseId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var a = await _db.Assignments.FindAsync(id);
        if (a == null) return NotFound();
        return View(a);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Assignment model)
    {
        if (id != model.Id) return BadRequest();
        if (model.MaxScore <= 0)
            ModelState.AddModelError(nameof(model.MaxScore), "Max score must be positive.");
        if (!ModelState.IsValid)
            return View(model);
        _db.Assignments.Update(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Assignment updated.";
        return RedirectToAction(nameof(Course), new { id = model.CourseId });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Assignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();
        return View(a);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var a = await _db.Assignments.FindAsync(id);
        if (a == null) return NotFound();
        var courseId = a.CourseId;
        _db.Assignments.Remove(a);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Assignment deleted.";
        return RedirectToAction(nameof(Course), new { id = courseId });
    }

    public async Task<IActionResult> Results(int id)
    {
        var assignment = await _db.Assignments.AsNoTracking()
            .Include(a => a.Course)
            .Include(a => a.Results)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (assignment == null) return NotFound();

        var studentIds = await _db.CourseEnrolments.AsNoTracking()
            .Where(e => e.CourseId == assignment.CourseId && e.Status == EnrolmentStatuses.Active)
            .Select(e => e.StudentProfileId)
            .ToListAsync();

        var existing = assignment.Results.ToDictionary(r => r.StudentProfileId);
        var students = await _db.StudentProfiles.AsNoTracking()
            .Where(s => studentIds.Contains(s.Id))
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.Assignment = assignment;
        ViewBag.Existing = existing;
        return View(students);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(int assignmentId, int studentProfileId, decimal score, string? feedback)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment == null) return NotFound();

        if (!await AccessQueries.StudentEnrolledInCourseAsync(_db, studentProfileId, assignment.CourseId))
            return Forbid();

        var err = AcademicRules.ValidateAssignmentScore(score, assignment.MaxScore);
        if (err != null)
        {
            TempData["Error"] = err;
            return RedirectToAction(nameof(Results), new { id = assignmentId });
        }

        var row = await _db.AssignmentResults
            .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.StudentProfileId == studentProfileId);
        if (row == null)
        {
            _db.AssignmentResults.Add(new AssignmentResult
            {
                AssignmentId = assignmentId,
                StudentProfileId = studentProfileId,
                Score = score,
                Feedback = feedback
            });
        }
        else
        {
            row.Score = score;
            row.Feedback = feedback;
        }

        await _db.SaveChangesAsync();
        TempData["Message"] = "Grade saved.";
        return RedirectToAction(nameof(Results), new { id = assignmentId });
    }
}
