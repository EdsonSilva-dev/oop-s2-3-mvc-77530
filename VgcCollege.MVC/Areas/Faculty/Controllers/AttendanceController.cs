using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Areas.Faculty.ViewModels;
using VgcCollege.MVC.Data;
using VgcCollege.MVC.Models;

namespace VgcCollege.MVC.Areas.Faculty.Controllers
{
    [Area("Faculty")]
    [Authorize(Roles = UserRoles.Faculty)]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int? courseId, bool? present)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();
            if (!assignedCourseIds.Any())
            {
                return View(new List<FacultyAttendanceIndexViewModel>());
            }

            var query = _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.Course)
                .Where(a => a.CourseEnrolment != null && assignedCourseIds.Contains(a.CourseEnrolment.CourseId))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.CourseEnrolment!.StudentProfile!.FullName.Contains(search) ||
                    a.CourseEnrolment.StudentProfile.StudentNumber.Contains(search));
            }

            if (courseId.HasValue)
            {
                query = query.Where(a => a.CourseEnrolment!.CourseId == courseId.Value);
            }

            if (present.HasValue)
            {
                query = query.Where(a => a.Present == present.Value);
            }

            ViewBag.Search = search;
            ViewBag.CourseId = courseId;
            ViewBag.Present = present;
            ViewBag.CourseOptions = await GetCourseOptionsAsync(assignedCourseIds);

            var items = await query
                .OrderByDescending(a => a.SessionDate)
                .Select(a => new FacultyAttendanceIndexViewModel
                {
                    Id = a.Id,
                    CourseEnrolmentId = a.CourseEnrolmentId,
                    StudentName = a.CourseEnrolment != null && a.CourseEnrolment.StudentProfile != null ? a.CourseEnrolment.StudentProfile.FullName : "",
                    StudentNumber = a.CourseEnrolment != null && a.CourseEnrolment.StudentProfile != null ? a.CourseEnrolment.StudentProfile.StudentNumber : "",
                    CourseCode = a.CourseEnrolment != null && a.CourseEnrolment.Course != null ? a.CourseEnrolment.Course.Code : "",
                    CourseName = a.CourseEnrolment != null && a.CourseEnrolment.Course != null ? a.CourseEnrolment.Course.Name : "",
                    SessionDate = a.SessionDate,
                    WeekNumber = a.WeekNumber,
                    Present = a.Present,
                    Notes = a.Notes
                })
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();

            var vm = new FacultyAttendanceCreateEditViewModel
            {
                SessionDate = DateTime.Today,
                WeekNumber = 1,
                Present = true,
                Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacultyAttendanceCreateEditViewModel model)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();
            await ValidateAttendanceAsync(model, assignedCourseIds);

            if (!ModelState.IsValid)
            {
                model.Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds);
                return View(model);
            }

            var entity = new AttendanceRecord
            {
                CourseEnrolmentId = model.CourseEnrolmentId,
                SessionDate = model.SessionDate,
                WeekNumber = model.WeekNumber,
                Present = model.Present,
                Notes = model.Notes
            };

            _context.AttendanceRecords.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance record created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();

            var entity = await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (entity == null || entity.CourseEnrolment == null || !assignedCourseIds.Contains(entity.CourseEnrolment.CourseId))
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var vm = new FacultyAttendanceCreateEditViewModel
            {
                Id = entity.Id,
                CourseEnrolmentId = entity.CourseEnrolmentId,
                SessionDate = entity.SessionDate,
                WeekNumber = entity.WeekNumber,
                Present = entity.Present,
                Notes = entity.Notes,
                Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacultyAttendanceCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var assignedCourseIds = await GetAssignedCourseIdsAsync();
            await ValidateAttendanceAsync(model, assignedCourseIds, model.Id);

            if (!ModelState.IsValid)
            {
                model.Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds);
                return View(model);
            }

            var entity = await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (entity == null || entity.CourseEnrolment == null || !assignedCourseIds.Contains(entity.CourseEnrolment.CourseId))
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            entity.CourseEnrolmentId = model.CourseEnrolmentId;
            entity.SessionDate = model.SessionDate;
            entity.WeekNumber = model.WeekNumber;
            entity.Present = model.Present;
            entity.Notes = model.Notes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance record updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateAttendanceAsync(FacultyAttendanceCreateEditViewModel model, List<int> assignedCourseIds, int? currentId = null)
        {
            var enrolment = await _context.CourseEnrolments
                .FirstOrDefaultAsync(e => e.Id == model.CourseEnrolmentId);

            if (enrolment == null || !assignedCourseIds.Contains(enrolment.CourseId))
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId), "Selected enrolment is not available to you.");
                return;
            }

            var duplicate = await _context.AttendanceRecords.AnyAsync(a =>
                a.CourseEnrolmentId == model.CourseEnrolmentId &&
                a.SessionDate.Date == model.SessionDate.Date &&
                (!currentId.HasValue || a.Id != currentId.Value));

            if (duplicate)
            {
                ModelState.AddModelError(nameof(model.SessionDate), "An attendance record for this date already exists.");
            }

            if (model.SessionDate > DateTime.Today.AddDays(1))
            {
                ModelState.AddModelError(nameof(model.SessionDate), "Session date cannot be too far in the future.");
            }
        }

        private async Task<List<int>> GetAssignedCourseIdsAsync()
        {
            var userId = _userManager.GetUserId(User);

            var facultyProfile = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

            if (facultyProfile == null)
            {
                return new List<int>();
            }

            return await _context.FacultyCourseAssignments
                .Where(fa => fa.FacultyProfileId == facultyProfile.Id)
                .Select(fa => fa.CourseId)
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetCourseOptionsAsync(List<int> assignedCourseIds)
        {
            return await _context.Courses
                .Where(c => assignedCourseIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Name}"
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetEnrolmentOptionsAsync(List<int> assignedCourseIds)
        {
            return await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                .Where(e => assignedCourseIds.Contains(e.CourseId))
                .OrderBy(e => e.StudentProfile!.FullName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.StudentProfile!.StudentNumber} - {e.StudentProfile.FullName} | {e.Course!.Code} - {e.Course.Name}"
                })
                .ToListAsync();
        }
    }
}
