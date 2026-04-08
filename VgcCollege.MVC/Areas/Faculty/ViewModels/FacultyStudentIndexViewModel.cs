namespace VgcCollege.MVC.Areas.Faculty.ViewModels
{
    public class FacultyStudentIndexViewModel
    {
        public int StudentProfileId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CourseEnrolmentId { get; set; }
        public string EnrolmentStatus { get; set; } = string.Empty;
    }
}