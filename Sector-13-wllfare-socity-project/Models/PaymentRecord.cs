using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class PaymentRecord : Base
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // "SSLCommerz", "Bank Transfer", "bKash", "Nagad", etc.
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string? TransactionId { get; set; }
        public string? PaymentProofUrl { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "Under Review", "Completed", "Failed"
        public string? Notes { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }

        // Navigation Properties
        public Order? Order { get; set; }
    }
}
