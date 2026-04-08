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
    public class ExamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? courseId, bool? released)
        {
            var query = _context.Exams
                .Include(e => e.Course)
                .AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(e => e.CourseId == courseId.Value);
            }

            if (released.HasValue)
            {
                query = query.Where(e => e.ResultsReleased == released.Value);
            }

            ViewBag.CourseId = courseId;
            ViewBag.Released = released;
            ViewBag.CourseOptions = await _context.Courses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Name}"
                })
                .ToListAsync();

            var exams = await query
                .OrderBy(e => e.ExamDate)
                .Select(e => new ExamIndexViewModel
                {
                    Id = e.Id,
                    CourseCode = e.Course != null ? e.Course.Code : "",
                    CourseName = e.Course != null ? e.Course.Name : "",
                    Title = e.Title,
                    ExamDate = e.ExamDate,
                    MaxScore = e.MaxScore,
                    ResultsReleased = e.ResultsReleased,
                    Weight = e.Weight
                })
                .ToListAsync();

            return View(exams);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new ExamCreateEditViewModel
            {
                ExamDate = DateTime.Today.AddMonths(1),
                MaxScore = 100,
                ResultsReleased = false,
                Courses = await GetCourseSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamCreateEditViewModel model)
        {
            await ValidateExamAsync(model);

            if (!ModelState.IsValid)
            {
                model.Courses = await GetCourseSelectListAsync();
                return View(model);
            }

            var exam = new Exam
            {
                CourseId = model.CourseId,
                Title = model.Title,
                ExamDate = model.ExamDate,
                MaxScore = model.MaxScore,
                ResultsReleased = model.ResultsReleased,
                Weight = model.Weight
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            var vm = new ExamCreateEditViewModel
            {
                Id = exam.Id,
                CourseId = exam.CourseId,
                Title = exam.Title,
                ExamDate = exam.ExamDate,
                MaxScore = exam.MaxScore,
                ResultsReleased = exam.ResultsReleased,
                Weight = exam.Weight,
                Courses = await GetCourseSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            await ValidateExamAsync(model);

            if (!ModelState.IsValid)
            {
                model.Courses = await GetCourseSelectListAsync();
                return View(model);
            }

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            exam.CourseId = model.CourseId;
            exam.Title = model.Title;
            exam.ExamDate = model.ExamDate;
            exam.MaxScore = model.MaxScore;
            exam.ResultsReleased = model.ResultsReleased;
            exam.Weight = model.Weight;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exam = await _context.Exams
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return NotFound();
            }

            if (exam.Results.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete an exam that already has results.";
                return RedirectToAction(nameof(Index));
            }

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRelease(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            exam.ResultsReleased = !exam.ResultsReleased;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = exam.ResultsReleased
                ? "Exam results released successfully."
                : "Exam results moved back to provisional.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateExamAsync(ExamCreateEditViewModel model)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == model.CourseId);
            if (!courseExists)
            {
                ModelState.AddModelError(nameof(model.CourseId), "Selected course does not exist.");
            }

            if (model.MaxScore <= 0)
            {
                ModelState.AddModelError(nameof(model.MaxScore), "Max Score must be greater than zero.");
            }

            if (model.Weight.HasValue && (model.Weight < 0 || model.Weight > 100))
            {
                ModelState.AddModelError(nameof(model.Weight), "Weight must be between 0 and 100.");
            }
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
    }
}