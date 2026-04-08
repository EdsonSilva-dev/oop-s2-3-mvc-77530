namespace VgcCollege.MVC.Areas.Student.ViewModels
{
    public class StudentAttendanceIndexViewModel
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public int WeekNumber { get; set; }
        public bool Present { get; set; }
        public string? Notes { get; set; }
    }
}