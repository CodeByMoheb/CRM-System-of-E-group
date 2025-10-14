using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class LeaveBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public required string LeaveType { get; set; }

        [Required]
        public int TotalEntitled { get; set; }

        [Required]
        public int Used { get; set; }

        [Required]
        public int Pending { get; set; }

        [Required]
        public int Remaining { get; set; }

        [Required]
        public int Year { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }
    }
}