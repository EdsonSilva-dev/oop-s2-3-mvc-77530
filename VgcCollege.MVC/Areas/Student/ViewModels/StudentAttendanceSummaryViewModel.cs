namespace VgcCollege.MVC.Areas.Student.ViewModels
{
    public class StudentAttendanceSummaryViewModel
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
}
