namespace VgcCollege.MVC.Areas.Student.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public int TotalEnrolments { get; set; }
        public int TotalAttendanceRecords { get; set; }
        public int TotalAssignmentResults { get; set; }
        public int TotalVisibleExamResults { get; set; }
    }
}