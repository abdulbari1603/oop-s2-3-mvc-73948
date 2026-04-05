using Microsoft.AspNetCore.Identity;

namespace VcgCollege.Web.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
