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
    public class EnrolmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrolmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? studentSearch, int? courseId, EnrolmentStatus? status)
        {
            var query = _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(studentSearch))
            {
                query = query.Where(e =>
                    e.StudentProfile!.FullName.Contains(studentSearch) ||
                    e.StudentProfile.StudentNumber.Contains(studentSearch));
            }

            if (courseId.HasValue)
            {
                query = query.Where(e => e.CourseId == courseId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            ViewBag.StudentSearch = studentSearch;
            ViewBag.CourseId = courseId;
            ViewBag.Status = status;
            ViewBag.CourseOptions = await _context.Courses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Name}"
                })
                .ToListAsync();

            ViewBag.StatusOptions = Enum.GetValues(typeof(EnrolmentStatus))
                .Cast<EnrolmentStatus>()
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                })
                .ToList();

            var enrolments = await query
                .OrderByDescending(e => e.EnrolDate)
                .Select(e => new EnrolmentIndexViewModel
                {
                    Id = e.Id,
                    StudentNumber = e.StudentProfile != null ? e.StudentProfile.StudentNumber : "",
                    StudentName = e.StudentProfile != null ? e.StudentProfile.FullName : "",
                    CourseCode = e.Course != null ? e.Course.Code : "",
                    CourseName = e.Course != null ? e.Course.Name : "",
                    BranchName = e.Course != null && e.Course.Branch != null ? e.Course.Branch.Name : "",
                    EnrolDate = e.EnrolDate,
                    Status = e.Status.ToString()
                })
                .ToListAsync();

            return View(enrolments);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new EnrolmentCreateEditViewModel
            {
                EnrolDate = DateTime.Today,
                Status = EnrolmentStatus.Active,
                Students = await GetStudentSelectListAsync(),
                Courses = await GetCourseSelectListAsync(),
                Statuses = GetStatusSelectList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnrolmentCreateEditViewModel model)
        {
            await ValidateEnrolmentAsync(model);

            if (!ModelState.IsValid)
            {
                model.Students = await GetStudentSelectListAsync();
                model.Courses = await GetCourseSelectListAsync();
                model.Statuses = GetStatusSelectList();
                return View(model);
            }

            var enrolment = new CourseEnrolment
            {
                StudentProfileId = model.StudentProfileId,
                CourseId = model.CourseId,
                EnrolDate = model.EnrolDate,
                Status = model.Status
            };

            _context.CourseEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Enrolment created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var enrolment = await _context.CourseEnrolments.FindAsync(id);
            if (enrolment == null)
            {
                return NotFound();
            }

            var vm = new EnrolmentCreateEditViewModel
            {
                Id = enrolment.Id,
                StudentProfileId = enrolment.StudentProfileId,
                CourseId = enrolment.CourseId,
                EnrolDate = enrolment.EnrolDate,
                Status = enrolment.Status,
                Students = await GetStudentSelectListAsync(),
                Courses = await GetCourseSelectListAsync(),
                Statuses = GetStatusSelectList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnrolmentCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            await ValidateEnrolmentAsync(model, model.Id);

            if (!ModelState.IsValid)
            {
                model.Students = await GetStudentSelectListAsync();
                model.Courses = await GetCourseSelectListAsync();
                model.Statuses = GetStatusSelectList();
                return View(model);
            }

            var enrolment = await _context.CourseEnrolments.FindAsync(id);
            if (enrolment == null)
            {
                return NotFound();
            }

            enrolment.StudentProfileId = model.StudentProfileId;
            enrolment.CourseId = model.CourseId;
            enrolment.EnrolDate = model.EnrolDate;
            enrolment.Status = model.Status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Enrolment updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var enrolment = await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Include(e => e.AttendanceRecords)
                .Include(e => e.AssignmentResults)
                .Include(e => e.ExamResults)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrolment == null)
            {
                return NotFound();
            }

            return View(enrolment);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var enrolment = await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrolment == null)
            {
                return NotFound();
            }

            return View(enrolment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrolment = await _context.CourseEnrolments
                .Include(e => e.AttendanceRecords)
                .Include(e => e.AssignmentResults)
                .Include(e => e.ExamResults)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrolment == null)
            {
                return NotFound();
            }

            if (enrolment.AttendanceRecords.Any() || enrolment.AssignmentResults.Any() || enrolment.ExamResults.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete an enrolment that already has attendance or results linked.";
                return RedirectToAction(nameof(Index));
            }

            _context.CourseEnrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Enrolment deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateEnrolmentAsync(EnrolmentCreateEditViewModel model, int? currentId = null)
        {
            var studentExists = await _context.StudentProfiles.AnyAsync(s => s.Id == model.StudentProfileId);
            if (!studentExists)
            {
                ModelState.AddModelError(nameof(model.StudentProfileId), "Selected student does not exist.");
            }

            var courseExists = await _context.Courses.AnyAsync(c => c.Id == model.CourseId);
            if (!courseExists)
            {
                ModelState.AddModelError(nameof(model.CourseId), "Selected course does not exist.");
            }

            var duplicateExists = await _context.CourseEnrolments.AnyAsync(e =>
                e.StudentProfileId == model.StudentProfileId &&
                e.CourseId == model.CourseId &&
                (!currentId.HasValue || e.Id != currentId.Value));

            if (duplicateExists)
            {
                ModelState.AddModelError(string.Empty, "This student is already enrolled in the selected course.");
            }

            if (model.EnrolDate > DateTime.Today.AddDays(1))
            {
                ModelState.AddModelError(nameof(model.EnrolDate), "Enrolment date cannot be too far in the future.");
            }
        }

        private async Task<List<SelectListItem>> GetStudentSelectListAsync()
        {
            return await _context.StudentProfiles
                .OrderBy(s => s.FullName)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.StudentNumber} - {s.FullName}"
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetCourseSelectListAsync()
        {
            return await _context.Courses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Name}"
                })
                .ToListAsync();
        }

        private List<SelectListItem> GetStatusSelectList()
        {
            return Enum.GetValues(typeof(EnrolmentStatus))
                .Cast<EnrolmentStatus>()
                .Select(status => new SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = status.ToString()
                })
                .ToList();
        }
    }
}