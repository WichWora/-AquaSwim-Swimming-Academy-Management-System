using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AQUA.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [Required]
        [MaxLength(10)]
        public string StudentId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(20)]
        public string? AttendanceStatus { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Schedule Schedule { get; set; }

        public virtual Student Student { get; set; }
    }
}