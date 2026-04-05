using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Models;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class StudentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.StudentProfiles.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        return View(list);
    }

    public IActionResult Create() => View(new StudentCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing != null)
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
            foreach (var err in create.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, RoleNames.Student);

        _db.StudentProfiles.Add(new StudentProfile
        {
            IdentityUserId = user.Id,
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            DateOfBirth = model.DateOfBirth,
            StudentNumber = model.StudentNumber
        });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Student created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var student = await _db.StudentProfiles.AsNoTracking()
            .Include(s => s.Enrolments).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        return View(student);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var student = await _db.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        var vm = new StudentEditViewModel
        {
            Id = student.Id,
            IdentityUserId = student.IdentityUserId,
            Email = student.Email,
            Name = student.Name,
            Phone = student.Phone,
            Address = student.Address,
            DateOfBirth = student.DateOfBirth,
            StudentNumber = student.StudentNumber
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
            return View(model);

        var student = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();

        var user = await _userManager.FindByIdAsync(student.IdentityUserId);
        if (user == null) return NotFound();

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var other = await _userManager.FindByEmailAsync(model.Email);
            if (other != null && other.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Email already in use.");
                return View(model);
            }
            user.Email = model.Email;
            user.UserName = model.Email;
        }
        user.DisplayName = model.Name;
        await _userManager.UpdateAsync(user);

        student.Name = model.Name;
        student.Email = model.Email;
        student.Phone = model.Phone;
        student.Address = model.Address;
        student.DateOfBirth = model.DateOfBirth;
        student.StudentNumber = model.StudentNumber;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Student updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var student = await _db.StudentProfiles.AsNoTracking()
            .Include(s => s.Enrolments)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _db.StudentProfiles
            .Include(s => s.Enrolments)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        if (student.Enrolments.Any())
        {
            TempData["Error"] = "Cannot delete a student with enrolments. Withdraw enrolments first.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var user = await _userManager.FindByIdAsync(student.IdentityUserId);
        _db.StudentProfiles.Remove(student);
        await _db.SaveChangesAsync();
        if (user != null)
            await _userManager.DeleteAsync(user);

        TempData["Message"] = "Student removed.";
        return RedirectToAction(nameof(Index));
    }
}
