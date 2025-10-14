using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Order : Base
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerAddress { get; set; }
        public string? CompanyName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } = "SSLCommerz";
        public string PaymentStatus { get; set; } = "Pending";
        public string OrderStatus { get; set; } = "Pending";
        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public ApplicationUser? User { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem : Base
    {
        public int OrderId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int Quantity { get; set; }
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
        public Order? Order { get; set; }
        public Service? Service { get; set; }
    }
}
