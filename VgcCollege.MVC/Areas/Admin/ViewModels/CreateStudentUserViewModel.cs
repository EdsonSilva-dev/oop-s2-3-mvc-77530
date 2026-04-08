using System.ComponentModel.DataAnnotations;

namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class CreateStudentUserViewModel
    {
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
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Temporary Password")]
        public string TemporaryPassword { get; set; } = "Student123!";

        [Display(Name = "Send with active profile")]
        public bool IsActive { get; set; } = true;
    }
}