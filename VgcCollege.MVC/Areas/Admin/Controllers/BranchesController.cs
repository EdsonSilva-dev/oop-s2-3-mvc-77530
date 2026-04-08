using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Data;

namespace VgcCollege.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class BranchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BranchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var branches = await _context.Branches
                .OrderBy(b => b.Name)
                .ToListAsync();

            return View(branches);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch)
        {
            if (await _context.Branches.AnyAsync(b => b.Name == branch.Name))
            {
                ModelState.AddModelError(nameof(branch.Name), "A branch with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(branch);
            }

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Branch created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Branch branch)
        {
            if (id != branch.Id)
            {
                return NotFound();
            }

            if (await _context.Branches.AnyAsync(b => b.Name == branch.Name && b.Id != branch.Id))
            {
                ModelState.AddModelError(nameof(branch.Name), "A branch with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(branch);
            }

            _context.Update(branch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Branch updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Courses)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Courses)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Courses)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return NotFound();
            }

            if (branch.Courses.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete a branch that still has courses assigned.";
                return RedirectToAction(nameof(Index));
            }

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Branch deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}