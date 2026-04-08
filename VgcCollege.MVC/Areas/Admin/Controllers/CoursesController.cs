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
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Branch)
                .OrderBy(c => c.Name)
                .Select(c => new CourseIndexViewModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    BranchName = c.Branch != null ? c.Branch.Name : "",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new CourseCreateEditViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(6),
                Branches = await GetBranchSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseCreateEditViewModel model)
        {
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "End Date must be after Start Date.");
            }

            if (await _context.Courses.AnyAsync(c => c.Code == model.Code))
            {
                ModelState.AddModelError(nameof(model.Code), "A course with this code already exists.");
            }

            if (!ModelState.IsValid)
            {
                model.Branches = await GetBranchSelectListAsync();
                return View(model);
            }

            var course = new Course
            {
                Code = model.Code,
                Name = model.Name,
                Description = model.Description,
                BranchId = model.BranchId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Course created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var vm = new CourseCreateEditViewModel
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                BranchId = course.BranchId,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                IsActive = course.IsActive,
                Branches = await GetBranchSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "End Date must be after Start Date.");
            }

            if (await _context.Courses.AnyAsync(c => c.Code == model.Code && c.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Code), "A course with this code already exists.");
            }

            if (!ModelState.IsValid)
            {
                model.Branches = await GetBranchSelectListAsync();
                return View(model);
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            course.Code = model.Code;
            course.Name = model.Name;
            course.Description = model.Description;
            course.BranchId = model.BranchId;
            course.StartDate = model.StartDate;
            course.EndDate = model.EndDate;
            course.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Course updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            if (course.Enrolments.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete a course that still has enrolments.";
                return RedirectToAction(nameof(Index));
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Course deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetBranchSelectListAsync()
        {
            return await _context.Branches
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                })
                .ToListAsync();
        }
    }
}