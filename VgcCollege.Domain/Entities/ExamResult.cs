using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VgcCollege.Domain.Entities
{
    public class ExamResult
    {
        public int Id { get; set; }

        [Required]
        public int ExamId { get; set; }

        public Exam? Exam { get; set; }

        [Required]
        public int CourseEnrolmentId { get; set; }

        public CourseEnrolment? CourseEnrolment { get; set; }

        [Range(0, 1000)]
        public decimal Score { get; set; }

        [StringLength(5)]
        public string? Grade { get; set; }

        [StringLength(500)]
        public string? Feedback { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
