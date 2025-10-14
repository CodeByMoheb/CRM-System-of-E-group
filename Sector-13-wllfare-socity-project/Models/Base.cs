using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Base
    {
        [Key]
        public int Id { get; set; }
    // Removed IsApproved to avoid EF Core mapping conflicts. Now only in derived classes where needed.
        public bool IsDelete { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public DateTime ApprovedAt { get; set; }
    }
}
