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
    public class ExamsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? examId)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();

            var query = _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e.Course)
                .Include(r => r.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .Where(r => r.Exam != null && assignedCourseIds.Contains(r.Exam.CourseId))
                .AsQueryable();

            if (examId.HasValue)
            {
                query = query.Where(r => r.ExamId == examId.Value);
            }

            ViewBag.ExamId = examId;
            ViewBag.ExamOptions = await _context.Exams
                .Include(e => e.Course)
                .Where(e => assignedCourseIds.Contains(e.CourseId))
                .OrderBy(e => e.Title)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Title} | {e.Course!.Code}"
                })
                .ToListAsync();

            var items = await query
                .OrderByDescending(r => r.RecordedAt)
                .Select(r => new FacultyExamResultIndexViewModel
                {
                    Id = r.Id,
                    ExamTitle = r.Exam != null ? r.Exam.Title : "",
                    CourseCode = r.Exam != null && r.Exam.Course != null ? r.Exam.Course.Code : "",
                    CourseName = r.Exam != null && r.Exam.Course != null ? r.Exam.Course.Name : "",
                    StudentName = r.CourseEnrolment != null && r.CourseEnrolment.StudentProfile != null ? r.CourseEnrolment.StudentProfile.FullName : "",
                    StudentNumber = r.CourseEnrolment != null && r.CourseEnrolment.StudentProfile != null ? r.CourseEnrolment.StudentProfile.StudentNumber : "",
                    Score = r.Score,
                    MaxScore = r.Exam != null ? r.Exam.MaxScore : 0,
                    Grade = r.Grade,
                    ResultsReleased = r.Exam != null && r.Exam.ResultsReleased,
                    RecordedAt = r.RecordedAt
                })
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> CreateResult()
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();

            var vm = new FacultyExamResultCreateEditViewModel
            {
                Exams = await GetExamOptionsAsync(assignedCourseIds),
                Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResult(FacultyExamResultCreateEditViewModel model)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();
            await ValidateExamResultAsync(model, assignedCourseIds);

            if (!ModelState.IsValid)
            {
                model.Exams = await GetExamOptionsAsync(assignedCourseIds);
                model.Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds);
                return View(model);
            }

            var entity = new ExamResult
            {
                ExamId = model.ExamId,
                CourseEnrolmentId = model.CourseEnrolmentId,
                Score = model.Score,
                Grade = model.Grade,
                Feedback = model.Feedback,
                RecordedAt = DateTime.UtcNow
            };

            _context.ExamResults.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam result created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditResult(int id)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();

            var entity = await _context.ExamResults
                .Include(r => r.Exam)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Exam == null || !assignedCourseIds.Contains(entity.Exam.CourseId))
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var vm = new FacultyExamResultCreateEditViewModel
            {
                Id = entity.Id,
                ExamId = entity.ExamId,
                CourseEnrolmentId = entity.CourseEnrolmentId,
                Score = entity.Score,
                Grade = entity.Grade,
                Feedback = entity.Feedback,
                Exams = await GetExamOptionsAsync(assignedCourseIds),
                Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditResult(int id, FacultyExamResultCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var assignedCourseIds = await GetAssignedCourseIdsAsync();
            await ValidateExamResultAsync(model, assignedCourseIds, model.Id);

            if (!ModelState.IsValid)
            {
                model.Exams = await GetExamOptionsAsync(assignedCourseIds);
                model.Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds);
                return View(model);
            }

            var entity = await _context.ExamResults
                .Include(r => r.Exam)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Exam == null || !assignedCourseIds.Contains(entity.Exam.CourseId))
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            entity.ExamId = model.ExamId;
            entity.CourseEnrolmentId = model.CourseEnrolmentId;
            entity.Score = model.Score;
            entity.Grade = model.Grade;
            entity.Feedback = model.Feedback;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam result updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateExamResultAsync(FacultyExamResultCreateEditViewModel model, List<int> assignedCourseIds, int? currentId = null)
        {
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == model.ExamId);
            var enrolment = await _context.CourseEnrolments.FirstOrDefaultAsync(e => e.Id == model.CourseEnrolmentId);

            if (exam == null || !assignedCourseIds.Contains(exam.CourseId))
            {
                ModelState.AddModelError(nameof(model.ExamId), "Selected exam is not available to you.");
                return;
            }

            if (enrolment == null || !assignedCourseIds.Contains(enrolment.CourseId))
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId), "Selected enrolment is not available to you.");
                return;
            }

            if (exam.CourseId != enrolment.CourseId)
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId), "Exam and enrolment must belong to the same course.");
            }

            if (model.Score < 0 || model.Score > exam.MaxScore)
            {
                ModelState.AddModelError(nameof(model.Score), $"Score must be between 0 and {exam.MaxScore}.");
            }

            var duplicate = await _context.ExamResults.AnyAsync(r =>
                r.ExamId == model.ExamId &&
                r.CourseEnrolmentId == model.CourseEnrolmentId &&
                (!currentId.HasValue || r.Id != currentId.Value));

            if (duplicate)
            {
                ModelState.AddModelError(string.Empty, "A result already exists for this exam and enrolment.");
            }
        }

        private async Task<List<int>> GetAssignedCourseIdsAsync()
        {
            var userId = _userManager.GetUserId(User);
            var facultyProfile = await _context.FacultyProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

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

        private async Task<List<SelectListItem>> GetExamOptionsAsync(List<int> assignedCourseIds)
        {
            return await _context.Exams
                .Include(e => e.Course)
                .Where(e => assignedCourseIds.Contains(e.CourseId))
                .OrderBy(e => e.Title)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Title} | {e.Course!.Code} - {e.Course.Name}"
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