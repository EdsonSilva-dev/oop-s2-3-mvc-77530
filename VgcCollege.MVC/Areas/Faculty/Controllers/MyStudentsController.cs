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
    public class MyStudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyStudentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int? courseId)
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
                .ToListAsync();

            var query = _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                .Where(e => assignedCourseIds.Contains(e.CourseId))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.StudentProfile!.FullName.Contains(search) ||
                    e.StudentProfile.StudentNumber.Contains(search));
            }

            if (courseId.HasValue)
            {
                query = query.Where(e => e.CourseId == courseId.Value);
            }

            ViewBag.Search = search;
            ViewBag.CourseId = courseId;
            ViewBag.CourseOptions = await _context.Courses
                .Where(c => assignedCourseIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Name}"
                })
                .ToListAsync();

            var students = await query
                .OrderBy(e => e.StudentProfile!.FullName)
                .Select(e => new FacultyStudentIndexViewModel
                {
                    StudentProfileId = e.StudentProfileId,
                    StudentNumber = e.StudentProfile != null ? e.StudentProfile.StudentNumber : "",
                    FullName = e.StudentProfile != null ? e.StudentProfile.FullName : "",
                    Email = e.StudentProfile != null ? e.StudentProfile.Email : "",
                    Phone = e.StudentProfile != null ? e.StudentProfile.Phone : "",
                    CourseCode = e.Course != null ? e.Course.Code : "",
                    CourseName = e.Course != null ? e.Course.Name : "",
                    CourseEnrolmentId = e.Id,
                    EnrolmentStatus = e.Status.ToString()
                })
                .ToListAsync();

            return View(students);
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

            var assignedCourseIds = await _context.FacultyCourseAssignments
                .Where(fa => fa.FacultyProfileId == facultyProfile.Id)
                .Select(fa => fa.CourseId)
                .ToListAsync();

            var student = await _context.StudentProfiles
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s =>
                    s.Id == id &&
                    s.Enrolments.Any(e => assignedCourseIds.Contains(e.CourseId)));

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            return View(student);
        }
    }
}