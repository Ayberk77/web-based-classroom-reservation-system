using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassroomReservationSystem.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public int ClassroomId { get; set; }

        [Required]
        public int InstructorId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("InstructorId")]
        public User? Instructor { get; set; }

        [ForeignKey("ClassroomId")]
        public Classroom? Classroom { get; set; }
    }
}
