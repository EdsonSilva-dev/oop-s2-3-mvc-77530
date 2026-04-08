namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class AssignmentIndexViewModel
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal MaxScore { get; set; }
        public DateTime DueDate { get; set; }
        public decimal? Weight { get; set; }
    }
}