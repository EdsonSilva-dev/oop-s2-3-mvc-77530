using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VgcCollege.Domain.Entities
{
    public class FacultyCourseAssignment
    {
        public int Id { get; set; }

        [Required]
        public int FacultyProfileId { get; set; }

        public FacultyProfile? FacultyProfile { get; set; }

        [Required]
        public int CourseId { get; set; }

        public Course? Course { get; set; }

        public bool IsTutor { get; set; } = false;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
