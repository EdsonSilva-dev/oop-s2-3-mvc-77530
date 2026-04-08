using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class AssignmentResultCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Assignment")]
        public int AssignmentId { get; set; }

        [Required]
        [Display(Name = "Enrolment")]
        public int CourseEnrolmentId { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal Score { get; set; }

        [StringLength(500)]
        public string? Feedback { get; set; }

        public List<SelectListItem> Assignments { get; set; } = new();
        public List<SelectListItem> Enrolments { get; set; } = new();
    }
}