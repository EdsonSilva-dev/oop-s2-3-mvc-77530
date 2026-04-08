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
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            var query = _context.Assignments
                .Include(a => a.Course)
                .AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(a => a.CourseId == courseId.Value);
            }

            ViewBag.CourseId = courseId;
            ViewBag.CourseOptions = await _context.Courses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Name}"
                })
                .ToListAsync();

            var assignments = await query
                .OrderBy(a => a.DueDate)
                .Select(a => new AssignmentIndexViewModel
                {
                    Id = a.Id,
                    CourseCode = a.Course != null ? a.Course.Code : "",
                    CourseName = a.Course != null ? a.Course.Name : "",
                    Title = a.Title,
                    MaxScore = a.MaxScore,
                    DueDate = a.DueDate,
                    Weight = a.Weight
                })
                .ToListAsync();

            return View(assignments);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new AssignmentCreateEditViewModel
            {
                DueDate = DateTime.Today.AddDays(14),
                MaxScore = 100,
                Courses = await GetCourseSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentCreateEditViewModel model)
        {
            await ValidateAssignmentAsync(model);

            if (!ModelState.IsValid)
            {
                model.Courses = await GetCourseSelectListAsync();
                return View(model);
            }

            var assignment = new Assignment
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                MaxScore = model.MaxScore,
                DueDate = model.DueDate,
                Weight = model.Weight
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Assignment created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            var vm = new AssignmentCreateEditViewModel
            {
                Id = assignment.Id,
                CourseId = assignment.CourseId,
                Title = assignment.Title,
                Description = assignment.Description,
                MaxScore = assignment.MaxScore,
                DueDate = assignment.DueDate,
                Weight = assignment.Weight,
                Courses = await GetCourseSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssignmentCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            await ValidateAssignmentAsync(model);

            if (!ModelState.IsValid)
            {
                model.Courses = await GetCourseSelectListAsync();
                return View(model);
            }

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            assignment.CourseId = model.CourseId;
            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.MaxScore = model.MaxScore;
            assignment.DueDate = model.DueDate;
            assignment.Weight = model.Weight;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Assignment updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Results)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Results)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Results)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            if (assignment.Results.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete an assignment that already has results.";
                return RedirectToAction(nameof(Index));
            }

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Assignment deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateAssignmentAsync(AssignmentCreateEditViewModel model)
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