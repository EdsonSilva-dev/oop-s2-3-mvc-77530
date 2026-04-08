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
    public class AssignmentResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? assignmentId)
        {
            var query = _context.AssignmentResults
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .AsQueryable();

            if (assignmentId.HasValue)
            {
                query = query.Where(r => r.AssignmentId == assignmentId.Value);
            }

            ViewBag.AssignmentId = assignmentId;
            ViewBag.AssignmentOptions = await _context.Assignments
                .Include(a => a.Course)
                .OrderBy(a => a.Title)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Title} - {a.Course!.Code}"
                })
                .ToListAsync();

            var results = await query
                .OrderByDescending(r => r.GradedAt)
                .Select(r => new AssignmentResultIndexViewModel
                {
                    Id = r.Id,
                    AssignmentTitle = r.Assignment != null ? r.Assignment.Title : "",
                    CourseName = r.Assignment != null && r.Assignment.Course != null ? r.Assignment.Course.Name : "",
                    StudentName = r.CourseEnrolment != null && r.CourseEnrolment.StudentProfile != null ? r.CourseEnrolment.StudentProfile.FullName : "",
                    StudentNumber = r.CourseEnrolment != null && r.CourseEnrolment.StudentProfile != null ? r.CourseEnrolment.StudentProfile.StudentNumber : "",
                    Score = r.Score,
                    MaxScore = r.Assignment != null ? r.Assignment.MaxScore : 0,
                    GradedAt = r.GradedAt,
                    Feedback = r.Feedback
                })
                .ToListAsync();

            return View(results);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new AssignmentResultCreateEditViewModel
            {
                Assignments = await GetAssignmentSelectListAsync(),
                Enrolments = await GetEnrolmentSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentResultCreateEditViewModel model)
        {
            await ValidateAssignmentResultAsync(model);

            if (!ModelState.IsValid)
            {
                model.Assignments = await GetAssignmentSelectListAsync();
                model.Enrolments = await GetEnrolmentSelectListAsync();
                return View(model);
            }

            var result = new AssignmentResult
            {
                AssignmentId = model.AssignmentId,
                CourseEnrolmentId = model.CourseEnrolmentId,
                Score = model.Score,
                Feedback = model.Feedback,
                GradedAt = DateTime.UtcNow
            };

            _context.AssignmentResults.Add(result);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Assignment result created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _context.AssignmentResults.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            var vm = new AssignmentResultCreateEditViewModel
            {
                Id = result.Id,
                AssignmentId = result.AssignmentId,
                CourseEnrolmentId = result.CourseEnrolmentId,
                Score = result.Score,
                Feedback = result.Feedback,
                Assignments = await GetAssignmentSelectListAsync(),
                Enrolments = await GetEnrolmentSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssignmentResultCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            await ValidateAssignmentResultAsync(model, model.Id);

            if (!ModelState.IsValid)
            {
                model.Assignments = await GetAssignmentSelectListAsync();
                model.Enrolments = await GetEnrolmentSelectListAsync();
                return View(model);
            }

            var result = await _context.AssignmentResults.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            result.AssignmentId = model.AssignmentId;
            result.CourseEnrolmentId = model.CourseEnrolmentId;
            result.Score = model.Score;
            result.Feedback = model.Feedback;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Assignment result updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _context.AssignmentResults
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _context.AssignmentResults
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
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
            var result = await _context.AssignmentResults.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            _context.AssignmentResults.Remove(result);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Assignment result deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateAssignmentResultAsync(AssignmentResultCreateEditViewModel model, int? currentId = null)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == model.AssignmentId);

            if (assignment == null)
            {
                ModelState.AddModelError(nameof(model.AssignmentId), "Selected assignment does not exist.");
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

            if (assignment.CourseId != enrolment.CourseId)
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId),
                    "The selected enrolment does not belong to the same course as the assignment.");
            }

            if (model.Score < 0 || model.Score > assignment.MaxScore)
            {
                ModelState.AddModelError(nameof(model.Score),
                    $"Score must be between 0 and {assignment.MaxScore}.");
            }

            var duplicateExists = await _context.AssignmentResults.AnyAsync(r =>
                r.AssignmentId == model.AssignmentId &&
                r.CourseEnrolmentId == model.CourseEnrolmentId &&
                (!currentId.HasValue || r.Id != currentId.Value));

            if (duplicateExists)
            {
                ModelState.AddModelError(string.Empty,
                    "A result already exists for this assignment and enrolment.");
            }
        }

        private async Task<List<SelectListItem>> GetAssignmentSelectListAsync()
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .OrderBy(a => a.Title)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Title} | {a.Course!.Code} - {a.Course.Name}"
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