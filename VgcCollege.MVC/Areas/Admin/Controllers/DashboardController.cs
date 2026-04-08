using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Areas.Admin.ViewModels;
using VgcCollege.MVC.Data;

namespace VgcCollege.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalBranches = await _context.Branches.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalStudents = await _context.StudentProfiles.CountAsync(),
                TotalFaculty = await _context.FacultyProfiles.CountAsync(),
                TotalEnrolments = await _context.CourseEnrolments.CountAsync(),
                TotalExamsPendingRelease = await _context.Exams.CountAsync(e => !e.ResultsReleased)
            };

            return View(vm);
        }
    }
}