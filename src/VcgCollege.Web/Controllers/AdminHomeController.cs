using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VcgCollege.Web.Authorization;

namespace VcgCollege.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class AdminHomeController : Controller
{
    public IActionResult Index() => View();
}
