using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Models;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class FacultyMgmtController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public FacultyMgmtController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.FacultyProfiles.AsNoTracking()
            .Include(f => f.CourseAssignments)
            .OrderBy(f => f.Name)
            .ToListAsync();
        return View(list);
    }

    public IActionResult Create() => View(new FacultyCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FacultyCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            DisplayName = model.Name
        };
        var create = await _userManager.CreateAsync(user, model.Password);
        if (!create.Succeeded)
        {
            foreach (var e in create.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, RoleNames.Faculty);
        _db.FacultyProfiles.Add(new FacultyProfile
        {
            IdentityUserId = user.Id,
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone
        });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Faculty user created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var faculty = await _db.FacultyProfiles.AsNoTracking()
            .Include(f => f.CourseAssignments).ThenInclude(a => a.Course)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (faculty == null) return NotFound();

        var courses = await _db.Courses.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.AssignCourseList = new SelectList(courses, "Id", "Name");
        return View(faculty);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var f = await _db.FacultyProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound();
        return View(new FacultyEditViewModel
        {
            Id = f.Id,
            IdentityUserId = f.IdentityUserId,
            Email = f.Email,
            Name = f.Name,
            Phone = f.Phone
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FacultyEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
            return View(model);

        var faculty = await _db.FacultyProfiles.FirstOrDefaultAsync(f => f.Id == id);
        if (faculty == null) return NotFound();
        var user = await _userManager.FindByIdAsync(faculty.IdentityUserId);
        if (user == null) return NotFound();

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _userManager.FindByEmailAsync(model.Email) is { } other && other.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Email already in use.");
                return View(model);
            }
            user.Email = model.Email;
            user.UserName = model.Email;
        }
        user.DisplayName = model.Name;
        await _userManager.UpdateAsync(user);

        faculty.Name = model.Name;
        faculty.Email = model.Email;
        faculty.Phone = model.Phone;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Faculty updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignCourse(int facultyId, int courseId)
    {
        var exists = await _db.FacultyCourseAssignments
            .AnyAsync(a => a.FacultyProfileId == facultyId && a.CourseId == courseId);
        if (!exists)
        {
            _db.FacultyCourseAssignments.Add(new FacultyCourseAssignment
            {
                FacultyProfileId = facultyId,
                CourseId = courseId
            });
            await _db.SaveChangesAsync();
            TempData["Message"] = "Course assigned.";
        }
        return RedirectToAction(nameof(Details), new { id = facultyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAssignment(int assignmentId, int facultyId)
    {
        var row = await _db.FacultyCourseAssignments.FindAsync(assignmentId);
        if (row != null)
        {
            _db.FacultyCourseAssignments.Remove(row);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Assignment removed.";
        }
        return RedirectToAction(nameof(Details), new { id = facultyId });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var f = await _db.FacultyProfiles.AsNoTracking()
            .Include(x => x.CourseAssignments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound();
        return View(f);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var f = await _db.FacultyProfiles
            .Include(x => x.CourseAssignments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound();
        if (f.CourseAssignments.Any())
        {
            TempData["Error"] = "Remove course assignments before deleting faculty.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var user = await _userManager.FindByIdAsync(f.IdentityUserId);
        _db.FacultyProfiles.Remove(f);
        await _db.SaveChangesAsync();
        if (user != null)
            await _userManager.DeleteAsync(user);

        TempData["Message"] = "Faculty removed.";
        return RedirectToAction(nameof(Index));
    }
}
