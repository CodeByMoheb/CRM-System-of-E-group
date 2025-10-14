using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class CartItem : Base
    {
        public string UserId { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal? TravelAllowance { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public int? WorkforceSize { get; set; }
        public int? ManDaysRequired { get; set; }
        public string? Location { get; set; }
        public string? ServiceConfiguration { get; set; }
        public string? SpecialRequirements { get; set; }

        // Navigation Properties
        public ApplicationUser? User { get; set; }
        public Service? Service { get; set; }
    }
}
