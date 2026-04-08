namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class AssignmentResultIndexViewModel
    {
        public int Id { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public DateTime GradedAt { get; set; }
        public string? Feedback { get; set; }
    }
}