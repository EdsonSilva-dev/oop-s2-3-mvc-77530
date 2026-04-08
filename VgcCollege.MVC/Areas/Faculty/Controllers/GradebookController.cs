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
    public class GradebookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GradebookController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Assignments(int? assignmentId)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();

            var query = _context.AssignmentResults
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .Where(r => r.Assignment != null && assignedCourseIds.Contains(r.Assignment.CourseId))
                .AsQueryable();

            if (assignmentId.HasValue)
            {
                query = query.Where(r => r.AssignmentId == assignmentId.Value);
            }

            ViewBag.AssignmentId = assignmentId;
            ViewBag.AssignmentOptions = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => assignedCourseIds.Contains(a.CourseId))
                .OrderBy(a => a.Title)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Title} | {a.Course!.Code}"
                })
                .ToListAsync();

            var items = await query
                .OrderByDescending(r => r.GradedAt)
                .Select(r => new FacultyAssignmentResultIndexViewModel
                {
                    Id = r.Id,
                    AssignmentTitle = r.Assignment != null ? r.Assignment.Title : "",
                    CourseCode = r.Assignment != null && r.Assignment.Course != null ? r.Assignment.Course.Code : "",
                    CourseName = r.Assignment != null && r.Assignment.Course != null ? r.Assignment.Course.Name : "",
                    StudentName = r.CourseEnrolment != null && r.CourseEnrolment.StudentProfile != null ? r.CourseEnrolment.StudentProfile.FullName : "",
                    StudentNumber = r.CourseEnrolment != null && r.CourseEnrolment.StudentProfile != null ? r.CourseEnrolment.StudentProfile.StudentNumber : "",
                    Score = r.Score,
                    MaxScore = r.Assignment != null ? r.Assignment.MaxScore : 0,
                    Feedback = r.Feedback,
                    GradedAt = r.GradedAt
                })
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> CreateAssignmentResult()
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();

            var vm = new FacultyAssignmentResultCreateEditViewModel
            {
                Assignments = await GetAssignmentOptionsAsync(assignedCourseIds),
                Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignmentResult(FacultyAssignmentResultCreateEditViewModel model)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();
            await ValidateAssignmentResultAsync(model, assignedCourseIds);

            if (!ModelState.IsValid)
            {
                model.Assignments = await GetAssignmentOptionsAsync(assignedCourseIds);
                model.Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds);
                return View(model);
            }

            var entity = new AssignmentResult
            {
                AssignmentId = model.AssignmentId,
                CourseEnrolmentId = model.CourseEnrolmentId,
                Score = model.Score,
                Feedback = model.Feedback,
                GradedAt = DateTime.UtcNow
            };

            _context.AssignmentResults.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Assignment result created successfully.";
            return RedirectToAction(nameof(Assignments));
        }

        public async Task<IActionResult> EditAssignmentResult(int id)
        {
            var assignedCourseIds = await GetAssignedCourseIdsAsync();

            var entity = await _context.AssignmentResults
                .Include(r => r.Assignment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Assignment == null || !assignedCourseIds.Contains(entity.Assignment.CourseId))
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var vm = new FacultyAssignmentResultCreateEditViewModel
            {
                Id = entity.Id,
                AssignmentId = entity.AssignmentId,
                CourseEnrolmentId = entity.CourseEnrolmentId,
                Score = entity.Score,
                Feedback = entity.Feedback,
                Assignments = await GetAssignmentOptionsAsync(assignedCourseIds),
                Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignmentResult(int id, FacultyAssignmentResultCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var assignedCourseIds = await GetAssignedCourseIdsAsync();
            await ValidateAssignmentResultAsync(model, assignedCourseIds, model.Id);

            if (!ModelState.IsValid)
            {
                model.Assignments = await GetAssignmentOptionsAsync(assignedCourseIds);
                model.Enrolments = await GetEnrolmentOptionsAsync(assignedCourseIds);
                return View(model);
            }

            var entity = await _context.AssignmentResults
                .Include(r => r.Assignment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Assignment == null || !assignedCourseIds.Contains(entity.Assignment.CourseId))
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            entity.AssignmentId = model.AssignmentId;
            entity.CourseEnrolmentId = model.CourseEnrolmentId;
            entity.Score = model.Score;
            entity.Feedback = model.Feedback;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Assignment result updated successfully.";
            return RedirectToAction(nameof(Assignments));
        }

        private async Task ValidateAssignmentResultAsync(FacultyAssignmentResultCreateEditViewModel model, List<int> assignedCourseIds, int? currentId = null)
        {
            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.Id == model.AssignmentId);
            var enrolment = await _context.CourseEnrolments.FirstOrDefaultAsync(e => e.Id == model.CourseEnrolmentId);

            if (assignment == null || !assignedCourseIds.Contains(assignment.CourseId))
            {
                ModelState.AddModelError(nameof(model.AssignmentId), "Selected assignment is not available to you.");
                return;
            }

            if (enrolment == null || !assignedCourseIds.Contains(enrolment.CourseId))
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId), "Selected enrolment is not available to you.");
                return;
            }

            if (assignment.CourseId != enrolment.CourseId)
            {
                ModelState.AddModelError(nameof(model.CourseEnrolmentId), "Assignment and enrolment must belong to the same course.");
            }

            if (model.Score < 0 || model.Score > assignment.MaxScore)
            {
                ModelState.AddModelError(nameof(model.Score), $"Score must be between 0 and {assignment.MaxScore}.");
            }

            var duplicate = await _context.AssignmentResults.AnyAsync(r =>
                r.AssignmentId == model.AssignmentId &&
                r.CourseEnrolmentId == model.CourseEnrolmentId &&
                (!currentId.HasValue || r.Id != currentId.Value));

            if (duplicate)
            {
                ModelState.AddModelError(string.Empty, "A result already exists for this assignment and enrolment.");
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

        private async Task<List<SelectListItem>> GetAssignmentOptionsAsync(List<int> assignedCourseIds)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .Where(a => assignedCourseIds.Contains(a.CourseId))
                .OrderBy(a => a.Title)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Title} | {a.Course!.Code} - {a.Course.Name}"
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