using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VgcCollege.MVC.Areas.Faculty.ViewModels
{
    public class FacultyExamResultCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Exam")]
        public int ExamId { get; set; }

        [Required]
        [Display(Name = "Enrolment")]
        public int CourseEnrolmentId { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal Score { get; set; }

        [StringLength(5)]
        public string? Grade { get; set; }

        [StringLength(500)]
        public string? Feedback { get; set; }

        public List<SelectListItem> Exams { get; set; } = new();
        public List<SelectListItem> Enrolments { get; set; } = new();
    }
}