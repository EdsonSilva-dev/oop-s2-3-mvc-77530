using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Areas.Admin.ViewModels;
using VgcCollege.MVC.Data;

namespace VgcCollege.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? studentSearch, int? courseId, bool? present)
        {
            var query = _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(studentSearch))
            {
                query = query.Where(a =>
                    a.CourseEnrolment!.StudentProfile!.FullName.Contains(studentSearch) ||
                    a.CourseEnrolment.StudentProfile.StudentNumber.Contains(studentSearch));
            }

            if (courseId.HasValue)
            {
                query = query.Where(a => a.CourseEnrolment!.CourseId == courseId.Value);
            }

            if (present.HasValue)
            {
                query = query.Where(a => a.Present == present.Value);
            }

            ViewBag.StudentSearch = studentSearch;
            ViewBag.CourseId = courseId;
            ViewBag.Present = present;

            ViewBag.CourseOptions = await _context.Courses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Name}"
                })
                .ToListAsync();

            var records = await query
                .OrderByDescending(a => a.SessionDate)
                .ThenBy(a => a.CourseEnrolment!.StudentProfile!.FullName)
                .Select(a => new AttendanceIndexViewModel
                {
                    Id = a.Id,
                    CourseEnrolmentId = a.CourseEnrolmentId,
                    StudentName = a.CourseEnrolment != null && a.CourseEnrolment.StudentProfile != null
                        ? a.CourseEnrolment.StudentProfile.FullName
                        : "",
                    StudentNumber = a.CourseEnrolment != null && a.CourseEnrolment.StudentProfile != null
                        ? a.CourseEnrolment.StudentProfile.StudentNumber
                        : "",
                    CourseName = a.CourseEnrolment != null && a.CourseEnrolment.Course != null
                        ? a.CourseEnrolment.Course.Name
                        : "",
                    CourseCode = a.CourseEnrolment != null && a.CourseEnrolment.Course != null
                        ? a.CourseEnrolment.Course.Code
                        : "",
                    SessionDate = a.SessionDate,
                    WeekNumber = a.WeekNumber,
                    Present = a.Present,
                    Notes = a.Notes
                })
                .ToListAsync();

            return View(records);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new AttendanceCreateEditViewModel
            {
                SessionDate = DateTime.Today,
                WeekNumber = 1,
                Present = true,
                Enrolments = await GetEnrolmentSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AttendanceCreateEditViewModel model)
        {
            await ValidateAttendanceAsync(model);

            if (!ModelState.IsValid)
            {
                model.Enrolments = await GetEnrolmentSelectListAsync();
                return View(model);
            }

            var record = new AttendanceRecord
            {
                CourseEnrolmentId = model.CourseEnrolmentId,
                SessionDate = model.SessionDate,
                WeekNumber = model.WeekNumber,
                Present = model.Present,
                Notes = model.Notes
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance record created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            var vm = new AttendanceCreateEditViewModel
            {
                Id = record.Id,
                CourseEnrolmentId = record.CourseEnrolmentId,
                SessionDate = record.SessionDate,
                WeekNumber = record.WeekNumber,
                Present = record.Present,
                Notes = record.Notes,
                Enrolments = await GetEnrolmentSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AttendanceCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            await ValidateAttendanceAsync(model, model.Id);

            if (!ModelState.IsValid)
            {
                model.Enrolments = await GetEnrolmentSelectListAsync();
                return View(model);
            }

            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            record.CourseEnrolmentId = model.CourseEnrolmentId;
            record.SessionDate = model.SessionDate;
            record.WeekNumber = model.WeekNumber;
            record.Present = model.Present;
            record.Notes = model.Notes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance record updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var record = await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateAttendanceAsync(AttendanceCreateEditViewModel model, int? currentId = null)
        {
            var enrolment = await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == model.CourseEnrolmentId);

            if (enrolment == null)
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId), "Selected enrolment does not exist.");
            }

            var duplicateSession = await _context.AttendanceRecords.AnyAsync(a =>
                a.CourseEnrolmentId == model.CourseEnrolmentId &&
                a.SessionDate.Date == model.SessionDate.Date &&
                (!currentId.HasValue || a.Id != currentId.Value));

            if (duplicateSession)
            {
                ModelState.AddModelError(nameof(model.SessionDate),
                    "There is already an attendance record for this enrolment on the selected date.");
            }

            if (model.SessionDate > DateTime.Today.AddDays(1))
            {
                ModelState.AddModelError(nameof(model.SessionDate), "Session date cannot be too far in the future.");
            }

            if (model.WeekNumber < 1 || model.WeekNumber > 52)
            {
                ModelState.AddModelError(nameof(model.WeekNumber), "Week number must be between 1 and 52.");
            }
        }

        private async Task<List<SelectListItem>> GetEnrolmentSelectListAsync()
        {
            return await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                .OrderBy(e => e.StudentProfile!.FullName)
                .ThenBy(e => e.Course!.Name)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.StudentProfile!.StudentNumber} - {e.StudentProfile.FullName} | {e.Course!.Code} - {e.Course.Name}"
                })
                .ToListAsync();
        }
    }
}