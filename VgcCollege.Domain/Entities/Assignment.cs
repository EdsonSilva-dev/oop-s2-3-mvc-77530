using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VgcCollege.Domain.Entities
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public Course? Course { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 1000)]
        public decimal MaxScore { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Range(0, 100)]
        public decimal? Weight { get; set; }

        public ICollection<AssignmentResult> Results { get; set; } = new List<AssignmentResult>();
    }
}
