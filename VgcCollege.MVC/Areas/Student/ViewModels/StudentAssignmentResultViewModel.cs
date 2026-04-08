namespace VgcCollege.MVC.Areas.Student.ViewModels
{
    public class StudentAssignmentResultViewModel
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Feedback { get; set; }
        public DateTime GradedAt { get; set; }
    }
}