namespace VgcCollege.MVC.Areas.Faculty.ViewModels
{
    public class FacultyAssignmentResultIndexViewModel
    {
        public int Id { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Feedback { get; set; }
        public DateTime GradedAt { get; set; }
    }
}