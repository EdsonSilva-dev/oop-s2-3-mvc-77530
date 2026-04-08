using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class StudentCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Linked Student User")]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        public List<SelectListItem> StudentUsers { get; set; } = new();
    }
}