namespace VgcCollege.MVC.Areas.Student.ViewModels
{
    public class StudentExamResultViewModel
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ExamTitle { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Grade { get; set; }
        public string? Feedback { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}