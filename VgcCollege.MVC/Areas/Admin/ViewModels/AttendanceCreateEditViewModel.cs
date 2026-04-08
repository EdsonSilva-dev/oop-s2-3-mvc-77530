using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class AttendanceCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Enrolment")]
        public int CourseEnrolmentId { get; set; }

        [Required]
        [Display(Name = "Session Date")]
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; }

        [Range(1, 52)]
        [Display(Name = "Week Number")]
        public int WeekNumber { get; set; }

        [Display(Name = "Present")]
        public bool Present { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }

        public List<SelectListItem> Enrolments { get; set; } = new();
    }
}