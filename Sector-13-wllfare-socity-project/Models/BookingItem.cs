using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class BookingItem
    {
        public int Id { get; set; }
        
        // Foreign Keys
        public int BookingId { get; set; }
    public int? ServiceId { get; set; }
        
        // Service details (snapshot at time of booking)
        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? ServiceDescription { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ServiceType { get; set; } = string.Empty;
        
        // Pricing details
        public decimal UnitPrice { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        public decimal Subtotal { get; set; }
        
        public decimal? TravelAllowance { get; set; }
        
        public decimal? VatAmount { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        [StringLength(10)]
        public string Currency { get; set; } = "USD";
        
        // BSCI specific fields
        public int? WorkforceSize { get; set; }
        
        public int? ManDaysRequired { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; } // Inside Dhaka, Outside Dhaka
        
        // Service-specific configuration
        [StringLength(2000)]
        public string? ServiceConfiguration { get; set; } // JSON for service-specific data
        
        // Navigation properties
        public virtual Booking? Booking { get; set; }
        public virtual Service? Service { get; set; }
    }
}
