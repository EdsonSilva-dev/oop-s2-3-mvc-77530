using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Areas.Student.ViewModels;
using VgcCollege.MVC.Data;
using VgcCollege.MVC.Models;

namespace VgcCollege.MVC.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = UserRoles.Student)]
    public class MyCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyCoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var courses = await _context.CourseEnrolments
                .Where(e => e.StudentProfileId == student.Id)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Branch)
                .OrderBy(e => e.Course!.Name)
                .Select(e => new StudentCourseIndexViewModel
                {
                    CourseEnrolmentId = e.Id,
                    CourseCode = e.Course != null ? e.Course.Code : "",
                    CourseName = e.Course != null ? e.Course.Name : "",
                    BranchName = e.Course != null && e.Course.Branch != null ? e.Course.Branch.Name : "",
                    EnrolDate = e.EnrolDate,
                    Status = e.Status.ToString()
                })
                .ToListAsync();

            return View(courses);
        }
    }
}