using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VgcCollege.MVC.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }

        [Authorize]
        public IActionResult RedirectByRole()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            if (User.IsInRole("Faculty"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Faculty" });
            }

            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Student" });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}