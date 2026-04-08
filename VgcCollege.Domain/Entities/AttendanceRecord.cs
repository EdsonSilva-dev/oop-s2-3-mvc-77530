using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VgcCollege.Domain.Entities
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        [Required]
        public int CourseEnrolmentId { get; set; }

        public CourseEnrolment? CourseEnrolment { get; set; }

        [Required]
        public DateTime SessionDate { get; set; }

        public int WeekNumber { get; set; }

        public bool Present { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }
    }
}
