namespace VgcCollege.MVC.Areas.Student.ViewModels
{
    public class StudentAttendanceIndexViewModel
    {
        public List<StudentAttendanceSummaryViewModel> Summaries { get; set; } = new();
        public List<StudentAttendanceRecordViewModel> Records { get; set; } = new();
    }

    public class StudentAttendanceRecordViewModel
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public int WeekNumber { get; set; }
        public bool Present { get; set; }
        public string? Notes { get; set; }
    }
}