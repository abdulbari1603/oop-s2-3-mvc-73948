using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data;

namespace VcgCollege.Web.Services;

public class UserContextAccessor : IUserContextAccessor
{
    private readonly IHttpContextAccessor _http;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public UserContextAccessor(
        IHttpContextAccessor http,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _http = http;
        _userManager = userManager;
        _db = db;
    }

    public async Task<int?> GetCurrentStudentProfileIdAsync(CancellationToken cancellationToken = default)
    {
        var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        if (!_http.HttpContext!.User.IsInRole(RoleNames.Student)) return null;

        var profile = await _db.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdentityUserId == userId, cancellationToken);
        return profile?.Id;
    }

    public async Task<int?> GetCurrentFacultyProfileIdAsync(CancellationToken cancellationToken = default)
    {
        var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        if (!_http.HttpContext!.User.IsInRole(RoleNames.Faculty)) return null;

        var profile = await _db.FacultyProfiles.AsNoTracking()
            .FirstOrDefaultAsync(f => f.IdentityUserId == userId, cancellationToken);
        return profile?.Id;
    }
}
