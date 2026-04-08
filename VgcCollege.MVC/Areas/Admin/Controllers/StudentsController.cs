using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Areas.Admin.ViewModels;
using VgcCollege.MVC.Data;
using VgcCollege.MVC.Models;

namespace VgcCollege.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _context.StudentProfiles
                .Include(s => s.Enrolments)
                .OrderBy(s => s.FullName)
                .Select(s => new StudentIndexViewModel
                {
                    Id = s.Id,
                    StudentNumber = s.StudentNumber,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    TotalEnrolments = s.Enrolments.Count
                })
                .ToListAsync();

            return View(students);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new StudentCreateEditViewModel
            {
                DateOfBirth = DateTime.Today.AddYears(-18),
                StudentUsers = await GetAvailableStudentUsersAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentCreateEditViewModel model)
        {
            await ValidateStudentAsync(model);

            if (!ModelState.IsValid)
            {
                model.StudentUsers = await GetAvailableStudentUsersAsync(model.ApplicationUserId);
                return View(model);
            }

            var student = new StudentProfile
            {
                ApplicationUserId = model.ApplicationUserId,
                StudentNumber = model.StudentNumber,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth
            };

            _context.StudentProfiles.Add(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student profile created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.StudentProfiles.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var vm = new StudentCreateEditViewModel
            {
                Id = student.Id,
                ApplicationUserId = student.ApplicationUserId,
                StudentNumber = student.StudentNumber,
                FullName = student.FullName,
                Email = student.Email,
                Phone = student.Phone,
                Address = student.Address,
                DateOfBirth = student.DateOfBirth,
                StudentUsers = await GetAvailableStudentUsersAsync(student.ApplicationUserId)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            await ValidateStudentAsync(model, model.Id);

            if (!ModelState.IsValid)
            {
                model.StudentUsers = await GetAvailableStudentUsersAsync(model.ApplicationUserId);
                return View(model);
            }

            var student = await _context.StudentProfiles.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            student.ApplicationUserId = model.ApplicationUserId;
            student.StudentNumber = model.StudentNumber;
            student.FullName = model.FullName;
            student.Email = model.Email;
            student.Phone = model.Phone;
            student.Address = model.Address;
            student.DateOfBirth = model.DateOfBirth;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateUser()
        {
            var vm = new CreateStudentUserViewModel
            {
                DateOfBirth = DateTime.Today.AddYears(-18),
                TemporaryPassword = "Student123!",
                IsActive = true
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateStudentUserViewModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
            }

            var emailExistsInProfile = await _context.StudentProfiles.AnyAsync(s => s.Email == model.Email);
            if (emailExistsInProfile)
            {
                ModelState.AddModelError(nameof(model.Email), "A student profile with this email already exists.");
            }

            if (model.DateOfBirth > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Date of birth cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.FullName,
                EmailConfirmed = true,
                IsActive = model.IsActive
            };

            var createUserResult = await _userManager.CreateAsync(user, model.TemporaryPassword);

            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, UserRoles.Student);

            var studentProfile = new StudentProfile
            {
                ApplicationUserId = user.Id,
                StudentNumber = await GenerateNextStudentNumberAsync(),
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth
            };

            _context.StudentProfiles.Add(studentProfile);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student user and profile created successfully. Student Number: {studentProfile.StudentNumber}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.StudentProfiles
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.StudentProfiles
                .Include(s => s.Enrolments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.StudentProfiles
                .Include(s => s.Enrolments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            if (student.Enrolments.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete a student profile that still has enrolments.";
                return RedirectToAction(nameof(Index));
            }

            _context.StudentProfiles.Remove(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student profile deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateStudentAsync(StudentCreateEditViewModel model, int? currentStudentId = null)
        {
            if (string.IsNullOrWhiteSpace(model.ApplicationUserId))
            {
                ModelState.AddModelError(nameof(model.ApplicationUserId), "Please select a student user.");
                return;
            }

            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.ApplicationUserId), "The selected user does not exist.");
            }
            else
            {
                var isStudent = await _userManager.IsInRoleAsync(user, UserRoles.Student);
                if (!isStudent)
                {
                    ModelState.AddModelError(nameof(model.ApplicationUserId), "The selected user must have the Student role.");
                }
            }

            var studentNumberExists = await _context.StudentProfiles
                .AnyAsync(s => s.StudentNumber == model.StudentNumber &&
                               (!currentStudentId.HasValue || s.Id != currentStudentId.Value));

            if (studentNumberExists)
            {
                ModelState.AddModelError(nameof(model.StudentNumber), "A student with this number already exists.");
            }

            var emailExists = await _context.StudentProfiles
                .AnyAsync(s => s.Email == model.Email &&
                               (!currentStudentId.HasValue || s.Id != currentStudentId.Value));

            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "A student with this email already exists.");
            }

            var userAlreadyLinked = await _context.StudentProfiles
                .AnyAsync(s => s.ApplicationUserId == model.ApplicationUserId &&
                               (!currentStudentId.HasValue || s.Id != currentStudentId.Value));

            if (userAlreadyLinked)
            {
                ModelState.AddModelError(nameof(model.ApplicationUserId), "This user is already linked to another student profile.");
            }

            if (model.DateOfBirth > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Date of birth cannot be in the future.");
            }
        }

        private async Task<List<SelectListItem>> GetAvailableStudentUsersAsync(string? selectedUserId = null)
        {
            var studentUsers = await _userManager.GetUsersInRoleAsync(UserRoles.Student);

            var linkedUserIds = await _context.StudentProfiles
                .Select(s => s.ApplicationUserId)
                .ToListAsync();

            var availableUsers = studentUsers
                .Where(u => !linkedUserIds.Contains(u.Id) || u.Id == selectedUserId)
                .OrderBy(u => u.Email)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.DisplayName ?? u.Email} ({u.Email})"
                })
                .ToList();

            return availableUsers;
        }

        private async Task<string> GenerateNextStudentNumberAsync()
        {
            var existingNumbers = await _context.StudentProfiles
                .Select(s => s.StudentNumber)
                .ToListAsync();

            var maxNumber = 0;

            foreach (var number in existingNumbers)
            {
                if (!string.IsNullOrWhiteSpace(number) &&
                    number.StartsWith("STU") &&
                    int.TryParse(number.Substring(3), out var parsed))
                {
                    if (parsed > maxNumber)
                    {
                        maxNumber = parsed;
                    }
                }
            }

            return $"STU{(maxNumber + 1):D3}";
        }
    }
}