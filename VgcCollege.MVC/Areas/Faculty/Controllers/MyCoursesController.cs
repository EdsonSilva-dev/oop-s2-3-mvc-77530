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
    public class MyCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyCoursesController(
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

            var courses = await _context.FacultyCourseAssignments
                .Where(fa => fa.FacultyProfileId == facultyProfile.Id)
                .Include(fa => fa.Course)
                    .ThenInclude(c => c!.Branch)
                .Select(fa => new FacultyCourseIndexViewModel
                {
                    CourseId = fa.CourseId,
                    CourseCode = fa.Course != null ? fa.Course.Code : "",
                    CourseName = fa.Course != null ? fa.Course.Name : "",
                    BranchName = fa.Course != null && fa.Course.Branch != null ? fa.Course.Branch.Name : "",
                    StartDate = fa.Course != null ? fa.Course.StartDate : DateTime.MinValue,
                    EndDate = fa.Course != null ? fa.Course.EndDate : DateTime.MinValue,
                    IsTutor = fa.IsTutor,
                    TotalStudents = fa.Course != null ? fa.Course.Enrolments.Count : 0
                })
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var facultyProfile = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

            if (facultyProfile == null)
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var isAssigned = await _context.FacultyCourseAssignments
                .AnyAsync(fa => fa.FacultyProfileId == facultyProfile.Id && fa.CourseId == id);

            if (!isAssigned)
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var course = await _context.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                    .ThenInclude(e => e.StudentProfile)
                .Include(c => c.Assignments)
                .Include(c => c.Exams)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
    }
}