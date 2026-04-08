namespace VgcCollege.MVC.Areas.Student.ViewModels
{
    public class StudentCourseIndexViewModel
    {
        public int CourseEnrolmentId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime EnrolDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}