namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class AttendanceIndexViewModel
    {
        public int Id { get; set; }
        public int CourseEnrolmentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public int WeekNumber { get; set; }
        public bool Present { get; set; }
        public string? Notes { get; set; }
    }
}