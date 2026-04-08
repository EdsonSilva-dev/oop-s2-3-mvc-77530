using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VgcCollege.Domain.Enums;

namespace VgcCollege.Domain.Entities
{
    public class CourseEnrolment
    {
        public int Id { get; set; }

        [Required]
        public int StudentProfileId { get; set; }

        public StudentProfile? StudentProfile { get; set; }

        [Required]
        public int CourseId { get; set; }

        public Course? Course { get; set; }

        [Required]
        public DateTime EnrolDate { get; set; } = DateTime.UtcNow;

        [Required]
        public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Active;

        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public ICollection<AssignmentResult> AssignmentResults { get; set; } = new List<AssignmentResult>();
        public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}
