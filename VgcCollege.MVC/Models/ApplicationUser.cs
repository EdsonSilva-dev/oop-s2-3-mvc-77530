using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VgcCollege.MVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(120)]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}