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

            var records = await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.Course)
                .Where(a => a.CourseEnrolment != null && a.CourseEnrolment.StudentProfileId == student.Id)
                .OrderByDescending(a => a.SessionDate)
                .Select(a => new StudentAttendanceIndexViewModel
                {
                    CourseCode = a.CourseEnrolment != null && a.CourseEnrolment.Course != null ? a.CourseEnrolment.Course.Code : "",
                    CourseName = a.CourseEnrolment != null && a.CourseEnrolment.Course != null ? a.CourseEnrolment.Course.Name : "",
                    SessionDate = a.SessionDate,
                    WeekNumber = a.WeekNumber,
                    Present = a.Present,
                    Notes = a.Notes
                })
                .ToListAsync();

            return View(records);
        }
    }
}