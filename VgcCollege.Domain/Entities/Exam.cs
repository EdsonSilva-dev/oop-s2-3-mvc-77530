using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VgcCollege.Domain.Entities
{
    public class Exam
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public Course? Course { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime ExamDate { get; set; }

        [Range(0, 1000)]
        public decimal MaxScore { get; set; }

        public bool ResultsReleased { get; set; } = false;

        [Range(0, 100)]
        public decimal? Weight { get; set; }

        public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
    }
}
