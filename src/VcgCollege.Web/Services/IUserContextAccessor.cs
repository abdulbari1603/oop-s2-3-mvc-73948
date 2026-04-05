namespace VcgCollege.Web.Services;

public interface IUserContextAccessor
{
    Task<int?> GetCurrentStudentProfileIdAsync(CancellationToken cancellationToken = default);
    Task<int?> GetCurrentFacultyProfileIdAsync(CancellationToken cancellationToken = default);
}
