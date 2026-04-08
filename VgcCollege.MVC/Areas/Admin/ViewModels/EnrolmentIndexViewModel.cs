namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class EnrolmentIndexViewModel
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime EnrolDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}