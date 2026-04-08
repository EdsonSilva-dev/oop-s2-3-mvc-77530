using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Areas.Faculty.ViewModels;
using VgcCollege.MVC.Data;
using VgcCollege.MVC.Models;

namespace VgcCollege.MVC.Areas.Faculty.Controllers
{
    [Area("Faculty")]
    [Authorize(Roles = UserRoles.Faculty)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var facultyProfile = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

            if (facultyProfile == null)
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var assignedCourseIds = await _context.FacultyCourseAssignments
                .Where(fa => fa.FacultyProfileId == facultyProfile.Id)
                .Select(fa => fa.CourseId)
                .Distinct()
                .ToListAsync();

            var totalStudents = await _context.CourseEnrolments
                .Where(e => assignedCourseIds.Contains(e.CourseId))
                .Select(e => e.StudentProfileId)
                .Distinct()
                .CountAsync();

            var vm = new FacultyDashboardViewModel
            {
                TotalAssignedCourses = assignedCourseIds.Count,
                TotalAssignedStudents = totalStudents,
                TotalAssignmentsManaged = await _context.Assignments.CountAsync(a => assignedCourseIds.Contains(a.CourseId)),
                TotalExamsManaged = await _context.Exams.CountAsync(e => assignedCourseIds.Contains(e.CourseId))
            };

            return View(vm);
        }
    }
}