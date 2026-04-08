namespace VgcCollege.MVC.Areas.Faculty.ViewModels
{
    public class FacultyCourseIndexViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsTutor { get; set; }
        public int TotalStudents { get; set; }
    }
}