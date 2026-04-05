using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class ExamsAdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public ExamsAdminController(ApplicationDbContext db) => _db = db;

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
            .Include(c => c.Exams)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    public IActionResult Create(int courseId) =>
        View(new Exam { CourseId = courseId, Date = DateTime.UtcNow.Date, MaxScore = 100, ResultsReleased = false });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Exam model)
    {
        if (model.MaxScore <= 0)
            ModelState.AddModelError(nameof(model.MaxScore), "Max score must be positive.");
        if (!ModelState.IsValid)
            return View(model);
        _db.Exams.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Exam created.";
        return RedirectToAction(nameof(Course), new { id = model.CourseId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var exam = await _db.Exams.FindAsync(id);
        if (exam == null) return NotFound();
        return View(exam);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Exam model)
    {
        if (id != model.Id) return BadRequest();
        if (model.MaxScore <= 0)
            ModelState.AddModelError(nameof(model.MaxScore), "Max score must be positive.");
        if (!ModelState.IsValid)
            return View(model);
        _db.Exams.Update(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Exam updated.";
        return RedirectToAction(nameof(Course), new { id = model.CourseId });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var exam = await _db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();
        return View(exam);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var exam = await _db.Exams.FindAsync(id);
        if (exam == null) return NotFound();
        var courseId = exam.CourseId;
        _db.Exams.Remove(exam);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Exam deleted.";
        return RedirectToAction(nameof(Course), new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRelease(int id)
    {
        var exam = await _db.Exams.FindAsync(id);
        if (exam == null) return NotFound();
        exam.ResultsReleased = !exam.ResultsReleased;
        await _db.SaveChangesAsync();
        TempData["Message"] = exam.ResultsReleased ? "Results released to students." : "Results hidden from students.";
        return RedirectToAction(nameof(Course), new { id = exam.CourseId });
    }

    public async Task<IActionResult> Results(int id)
    {
        var exam = await _db.Exams.AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.Results)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();

        var studentIds = await _db.CourseEnrolments.AsNoTracking()
            .Where(e => e.CourseId == exam.CourseId && e.Status == EnrolmentStatuses.Active)
            .Select(e => e.StudentProfileId)
            .ToListAsync();

        var existing = exam.Results.ToDictionary(r => r.StudentProfileId);
        var students = await _db.StudentProfiles.AsNoTracking()
            .Where(s => studentIds.Contains(s.Id))
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.Exam = exam;
        ViewBag.Existing = existing;
        return View(students);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(int examId, int studentProfileId, decimal score, string? grade)
    {
        var exam = await _db.Exams.FirstOrDefaultAsync(e => e.Id == examId);
        if (exam == null) return NotFound();

        if (!await AccessQueries.StudentEnrolledInCourseAsync(_db, studentProfileId, exam.CourseId))
            return Forbid();

        var err = AcademicRules.ValidateExamScore(score, exam.MaxScore);
        if (err != null)
        {
            TempData["Error"] = err;
            return RedirectToAction(nameof(Results), new { id = examId });
        }

        var letter = string.IsNullOrWhiteSpace(grade) ? AcademicRules.ComputeLetterGrade(score, exam.MaxScore) : grade;

        var row = await _db.ExamResults
            .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentProfileId == studentProfileId);
        if (row == null)
        {
            _db.ExamResults.Add(new ExamResult
            {
                ExamId = examId,
                StudentProfileId = studentProfileId,
                Score = score,
                Grade = letter
            });
        }
        else
        {
            row.Score = score;
            row.Grade = letter;
        }

        await _db.SaveChangesAsync();
        TempData["Message"] = "Exam result saved.";
        return RedirectToAction(nameof(Results), new { id = examId });
    }
}
