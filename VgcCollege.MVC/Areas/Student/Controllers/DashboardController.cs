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

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var enrolmentIds = await _context.CourseEnrolments
                .Where(e => e.StudentProfileId == student.Id)
                .Select(e => e.Id)
                .ToListAsync();

            var vm = new StudentDashboardViewModel
            {
                FullName = student.FullName,
                StudentNumber = student.StudentNumber,
                TotalEnrolments = enrolmentIds.Count,
                TotalAttendanceRecords = await _context.AttendanceRecords.CountAsync(a => enrolmentIds.Contains(a.CourseEnrolmentId)),
                TotalAssignmentResults = await _context.AssignmentResults.CountAsync(r => enrolmentIds.Contains(r.CourseEnrolmentId)),
                TotalVisibleExamResults = await _context.ExamResults.CountAsync(r =>
                    enrolmentIds.Contains(r.CourseEnrolmentId) &&
                    r.Exam != null &&
                    r.Exam.ResultsReleased)
            };

            return View(vm);
        }
    }
}