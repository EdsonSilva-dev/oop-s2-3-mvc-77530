using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class AssignmentCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Max Score")]
        public decimal MaxScore { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Range(0, 100)]
        public decimal? Weight { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();
    }
}