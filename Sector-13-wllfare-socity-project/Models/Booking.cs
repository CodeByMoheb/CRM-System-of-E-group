using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BookingNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? CustomerAddress { get; set; }
        
        [StringLength(200)]
        public string? CompanyName { get; set; }
        
        [StringLength(500)]
        public string? SpecialRequirements { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";
        
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Manual, SSLCommerz
        
        public string? TransactionId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Cancelled
        
        public DateTime BookingDate { get; set; } = DateTime.Now;
        
        public DateTime? PaymentDate { get; set; }
        
        public DateTime? ServiceDate { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        [StringLength(50)]
        public string? BookingStatus { get; set; } = "Pending"; // Pending, Confirmed, InProgress, Completed, Cancelled
        
        // Navigation properties
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}


