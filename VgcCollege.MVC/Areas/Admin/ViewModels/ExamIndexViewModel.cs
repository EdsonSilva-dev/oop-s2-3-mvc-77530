namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class ExamIndexViewModel
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public decimal MaxScore { get; set; }
        public bool ResultsReleased { get; set; }
        public decimal? Weight { get; set; }
    }
}