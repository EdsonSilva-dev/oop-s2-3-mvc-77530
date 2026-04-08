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
    public class MyAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyAttendanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

            var rawRecords = await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.Course)
                .Where(a => a.CourseEnrolment != null && a.CourseEnrolment.StudentProfileId == student.Id)
                .OrderByDescending(a => a.SessionDate)
                .ToListAsync();

            var summaries = rawRecords
                .Where(a => a.CourseEnrolment?.Course != null)
                .GroupBy(a => new
                {
                    a.CourseEnrolment!.Course!.Code,
                    a.CourseEnrolment.Course.Name
                })
                .Select(g => new StudentAttendanceSummaryViewModel
                {
                    CourseCode = g.Key.Code,
                    CourseName = g.Key.Name,
                    TotalSessions = g.Count(),
                    PresentCount = g.Count(x => x.Present),
                    AttendancePercentage = g.Count() == 0 ? 0 : Math.Round((decimal)g.Count(x => x.Present) / g.Count() * 100, 2)
                })
                .OrderBy(s => s.CourseName)
                .ToList();

            var records = rawRecords
                .Select(a => new StudentAttendanceRecordViewModel
                {
                    CourseCode = a.CourseEnrolment != null && a.CourseEnrolment.Course != null ? a.CourseEnrolment.Course.Code : "",
                    CourseName = a.CourseEnrolment != null && a.CourseEnrolment.Course != null ? a.CourseEnrolment.Course.Name : "",
                    SessionDate = a.SessionDate,
                    WeekNumber = a.WeekNumber,
                    Present = a.Present,
                    Notes = a.Notes
                })
                .ToList();

            var vm = new StudentAttendanceIndexViewModel
            {
                Summaries = summaries,
                Records = records
            };

            return View(vm);
        }
    }
}