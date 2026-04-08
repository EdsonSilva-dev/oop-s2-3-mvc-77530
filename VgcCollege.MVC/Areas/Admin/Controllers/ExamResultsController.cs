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
    public class ExamResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? examId)
        {
            var query = _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e.Course)
                .Include(r => r.CourseEnrolment)
                    .ThenInclude(en => en.StudentProfile)
                .AsQueryable();

            if (examId.HasValue)
            {
                query = query.Where(r => r.ExamId == examId.Value);
            }

            ViewBag.ExamId = examId;
            ViewBag.ExamOptions = await _context.Exams
                .Include(e => e.Course)
                .OrderBy(e => e.Title)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Title} - {e.Course!.Code}"
                })
                .ToListAsync();

            var results = await query
                .OrderByDescending(r => r.RecordedAt)
                .Select(r => new ExamResultIndexViewModel
                {
                    Id = r.Id,
                    ExamTitle = r.Exam != null ? r.Exam.Title : "",
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

            return View(results);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new ExamResultCreateEditViewModel
            {
                Exams = await GetExamSelectListAsync(),
                Enrolments = await GetEnrolmentSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamResultCreateEditViewModel model)
        {
            await ValidateExamResultAsync(model);

            if (!ModelState.IsValid)
            {
                model.Exams = await GetExamSelectListAsync();
                model.Enrolments = await GetEnrolmentSelectListAsync();
                return View(model);
            }

            var result = new ExamResult
            {
                ExamId = model.ExamId,
                CourseEnrolmentId = model.CourseEnrolmentId,
                Score = model.Score,
                Grade = model.Grade,
                Feedback = model.Feedback,
                RecordedAt = DateTime.UtcNow
            };

            _context.ExamResults.Add(result);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam result created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _context.ExamResults.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            var vm = new ExamResultCreateEditViewModel
            {
                Id = result.Id,
                ExamId = result.ExamId,
                CourseEnrolmentId = result.CourseEnrolmentId,
                Score = result.Score,
                Grade = result.Grade,
                Feedback = result.Feedback,
                Exams = await GetExamSelectListAsync(),
                Enrolments = await GetEnrolmentSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamResultCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            await ValidateExamResultAsync(model, model.Id);

            if (!ModelState.IsValid)
            {
                model.Exams = await GetExamSelectListAsync();
                model.Enrolments = await GetEnrolmentSelectListAsync();
                return View(model);
            }

            var result = await _context.ExamResults.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            result.ExamId = model.ExamId;
            result.CourseEnrolmentId = model.CourseEnrolmentId;
            result.Score = model.Score;
            result.Grade = model.Grade;
            result.Feedback = model.Feedback;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam result updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e.Course)
                .Include(r => r.CourseEnrolment)
                    .ThenInclude(en => en.StudentProfile)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e.Course)
                .Include(r => r.CourseEnrolment)
                    .ThenInclude(en => en.StudentProfile)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _context.ExamResults.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            _context.ExamResults.Remove(result);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam result deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateExamResultAsync(ExamResultCreateEditViewModel model, int? currentId = null)
        {
            var exam = await _context.Exams
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == model.ExamId);

            if (exam == null)
            {
                ModelState.AddModelError(nameof(model.ExamId), "Selected exam does not exist.");
                return;
            }

            var enrolment = await _context.CourseEnrolments
                .Include(e => e.Course)
                .Include(e => e.StudentProfile)
                .FirstOrDefaultAsync(e => e.Id == model.CourseEnrolmentId);

            if (enrolment == null)
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId), "Selected enrolment does not exist.");
                return;
            }

            if (exam.CourseId != enrolment.CourseId)
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId),
                    "The selected enrolment does not belong to the same course as the exam.");
            }

            if (model.Score < 0 || model.Score > exam.MaxScore)
            {
                ModelState.AddModelError(nameof(model.Score),
                    $"Score must be between 0 and {exam.MaxScore}.");
            }

            var duplicateExists = await _context.ExamResults.AnyAsync(r =>
                r.ExamId == model.ExamId &&
                r.CourseEnrolmentId == model.CourseEnrolmentId &&
                (!currentId.HasValue || r.Id != currentId.Value));

            if (duplicateExists)
            {
                ModelState.AddModelError(string.Empty,
                    "A result already exists for this exam and enrolment.");
            }
        }

        private async Task<List<SelectListItem>> GetExamSelectListAsync()
        {
            return await _context.Exams
                .Include(e => e.Course)
                .OrderBy(e => e.Title)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Title} | {e.Course!.Code} - {e.Course.Name}"
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetEnrolmentSelectListAsync()
        {
            return await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
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