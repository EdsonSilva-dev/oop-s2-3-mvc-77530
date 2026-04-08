using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Domain.Enums;

namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class EnrolmentCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentProfileId { get; set; }

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Enrolment Date")]
        [DataType(DataType.Date)]
        public DateTime EnrolDate { get; set; }

        [Required]
        public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Active;

        public List<SelectListItem> Students { get; set; } = new();
        public List<SelectListItem> Courses { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new();
    }
}