namespace VgcCollege.MVC.Areas.Admin.ViewModels
{
    public class StudentIndexViewModel
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int TotalEnrolments { get; set; }
    }
}