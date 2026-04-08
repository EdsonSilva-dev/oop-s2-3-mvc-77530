namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class ExamResultIndexViewModel
    {
        public int Id { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
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