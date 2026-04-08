namespace VgcCollege.MVC.Areas.Faculty.ViewModels
{
    public class FacultyExamResultIndexViewModel
    {
        public int Id { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Grade { get; set; }
        public bool ResultsReleased { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}