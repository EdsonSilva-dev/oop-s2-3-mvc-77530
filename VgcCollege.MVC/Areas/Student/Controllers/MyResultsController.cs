using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Areas.Student.ViewModels;
using VgcCollege.MVC.Data;
using VgcCollege.MVC.Models;

namespace VgcCollege.MVC.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = UserRoles.Student)]
    public class MyResultsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyResultsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Assignments()
        {
            var userId = _userManager.GetUserId(User);

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var results = await _context.AssignmentResults
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.CourseEnrolment)
                .Where(r => r.CourseEnrolment != null && r.CourseEnrolment.StudentProfileId == student.Id)
                .OrderByDescending(r => r.GradedAt)
                .Select(r => new StudentAssignmentResultViewModel
                {
                    CourseCode = r.Assignment != null && r.Assignment.Course != null ? r.Assignment.Course.Code : "",
                    CourseName = r.Assignment != null && r.Assignment.Course != null ? r.Assignment.Course.Name : "",
                    AssignmentTitle = r.Assignment != null ? r.Assignment.Title : "",
                    Score = r.Score,
                    MaxScore = r.Assignment != null ? r.Assignment.MaxScore : 0,
                    Feedback = r.Feedback,
                    GradedAt = r.GradedAt
                })
                .ToListAsync();

            return View(results);
        }

        public async Task<IActionResult> Exams()
        {
            var userId = _userManager.GetUserId(User);

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Home", new { area = "" });
            }

            var results = await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e.Course)
                .Include(r => r.CourseEnrolment)
                .Where(r =>
                    r.CourseEnrolment != null &&
                    r.CourseEnrolment.StudentProfileId == student.Id &&
                    r.Exam != null &&
                    r.Exam.ResultsReleased)
                .OrderByDescending(r => r.RecordedAt)
                .Select(r => new StudentExamResultViewModel
                {
                    CourseCode = r.Exam != null && r.Exam.Course != null ? r.Exam.Course.Code : "",
                    CourseName = r.Exam != null && r.Exam.Course != null ? r.Exam.Course.Name : "",
                    ExamTitle = r.Exam != null ? r.Exam.Title : "",
                    Score = r.Score,
                    MaxScore = r.Exam != null ? r.Exam.MaxScore : 0,
                    Grade = r.Grade,
                    Feedback = r.Feedback,
                    RecordedAt = r.RecordedAt
                })
                .ToListAsync();

            return View(results);
        }
    }
}