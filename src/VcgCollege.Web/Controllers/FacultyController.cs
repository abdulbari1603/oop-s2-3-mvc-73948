using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Faculty)]
public class FacultyController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IUserContextAccessor _userContext;

    public FacultyController(ApplicationDbContext db, IUserContextAccessor userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<IActionResult> Index()
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();

        var courses = await _db.FacultyCourseAssignments.AsNoTracking()
            .Where(a => a.FacultyProfileId == fid.Value)
            .Include(a => a.Course).ThenInclude(c => c.Branch)
            .Select(a => a.Course)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(courses);
    }

    public async Task<IActionResult> Students(int courseId)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, courseId))
            return Forbid();

        var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null) return NotFound();

        var enrolments = await _db.CourseEnrolments.AsNoTracking()
            .Where(e => e.CourseId == courseId && e.Status == EnrolmentStatuses.Active)
            .Include(e => e.Student)
            .OrderBy(e => e.Student!.Name)
            .ToListAsync();

        ViewBag.CourseName = course.Name;
        return View(enrolments);
    }

    public async Task<IActionResult> Gradebook(int courseId)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, courseId))
            return Forbid();

        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null) return NotFound();
        return View(course);
    }

    public async Task<IActionResult> AssignmentResults(int id)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();

        var assignment = await _db.Assignments.AsNoTracking()
            .Include(a => a.Course)
            .Include(a => a.Results)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (assignment == null) return NotFound();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, assignment.CourseId))
            return Forbid();

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
    public async Task<IActionResult> SaveAssignmentResult(int assignmentId, int studentProfileId, decimal score, string? feedback)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();

        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment == null) return NotFound();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, assignment.CourseId))
            return Forbid();

        if (!await AccessQueries.StudentEnrolledInCourseAsync(_db, studentProfileId, assignment.CourseId))
            return Forbid();

        var err = AcademicRules.ValidateAssignmentScore(score, assignment.MaxScore);
        if (err != null)
        {
            TempData["Error"] = err;
            return RedirectToAction(nameof(AssignmentResults), new { id = assignmentId });
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
        return RedirectToAction(nameof(AssignmentResults), new { id = assignmentId });
    }

    public async Task<IActionResult> Attendance(int courseId)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, courseId))
            return Forbid();

        var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null) return NotFound();

        var enrolments = await _db.CourseEnrolments.AsNoTracking()
            .Where(e => e.CourseId == courseId && e.Status == EnrolmentStatuses.Active)
            .Include(e => e.Student)
            .OrderBy(e => e.Student!.Name)
            .ToListAsync();

        ViewBag.CourseName = course.Name;
        return View(enrolments);
    }

    public async Task<IActionResult> AttendanceEnrolment(int id)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();

        var enrol = await _db.CourseEnrolments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.AttendanceRecords)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (enrol == null) return NotFound();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, enrol.CourseId))
            return Forbid();

        return View(enrol);
    }

    public async Task<IActionResult> CreateAttendance(int enrolmentId)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();

        var enrol = await _db.CourseEnrolments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == enrolmentId);
        if (enrol == null) return NotFound();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, enrol.CourseId))
            return Forbid();

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
    public async Task<IActionResult> CreateAttendance(AttendanceRecord model)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();

        var enrol = await _db.CourseEnrolments.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.CourseEnrolmentId);
        if (enrol == null) return NotFound();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, enrol.CourseId))
            return Forbid();

        if (await _db.AttendanceRecords.AnyAsync(a =>
                a.CourseEnrolmentId == model.CourseEnrolmentId && a.WeekNumber == model.WeekNumber))
        {
            ModelState.AddModelError(nameof(model.WeekNumber), "Week number already recorded.");
        }

        if (!ModelState.IsValid)
            return View(model);

        _db.AttendanceRecords.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Attendance saved.";
        return RedirectToAction(nameof(AttendanceEnrolment), new { id = model.CourseEnrolmentId });
    }

    public async Task<IActionResult> ExamResults(int id)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();

        var exam = await _db.Exams.AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.Results)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, exam.CourseId))
            return Forbid();

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
    public async Task<IActionResult> SaveExamResult(int examId, int studentProfileId, decimal score, string? grade)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();

        var exam = await _db.Exams.FirstOrDefaultAsync(e => e.Id == examId);
        if (exam == null) return NotFound();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, exam.CourseId))
            return Forbid();

        if (!await AccessQueries.StudentEnrolledInCourseAsync(_db, studentProfileId, exam.CourseId))
            return Forbid();

        var err = AcademicRules.ValidateExamScore(score, exam.MaxScore);
        if (err != null)
        {
            TempData["Error"] = err;
            return RedirectToAction(nameof(ExamResults), new { id = examId });
        }

        var letter = string.IsNullOrWhiteSpace(grade) ? AcademicRules.ComputeLetterGrade(score, exam.MaxScore) : grade;

        var row = await _db.ExamResults
            .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentProfileId == studentProfileId);
        if (row == null)
            _db.ExamResults.Add(new ExamResult { ExamId = examId, StudentProfileId = studentProfileId, Score = score, Grade = letter });
        else
        {
            row.Score = score;
            row.Grade = letter;
        }

        await _db.SaveChangesAsync();
        TempData["Message"] = "Exam result saved.";
        return RedirectToAction(nameof(ExamResults), new { id = examId });
    }

    public async Task<IActionResult> Exams(int courseId)
    {
        var fid = await _userContext.GetCurrentFacultyProfileIdAsync();
        if (fid == null) return Forbid();
        if (!await AccessQueries.FacultyTeachesCourseAsync(_db, fid.Value, courseId))
            return Forbid();

        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Exams)
            .FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null) return NotFound();
        return View(course);
    }
}
