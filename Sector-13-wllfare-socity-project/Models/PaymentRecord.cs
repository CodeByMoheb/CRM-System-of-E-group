using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class PaymentRecord : Base
    {
        public int OrderId { get; set; }
        public int? BookingId { get; set; } // For booking payments
        public string PaymentMethod { get; set; } = string.Empty; // "SSLCommerz", "Bank Transfer", "bKash", "Nagad", etc.
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string? TransactionId { get; set; }
        public string? PaymentProofUrl { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "Under Review", "Completed", "Failed", "Rejected"
        public string? Notes { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
        
        // Approval workflow fields
        public bool RequiresApproval { get; set; } = false; // True for manual bank transfers
    public string? ApprovalStatus { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"
    public new string? ApprovedBy { get; set; }
    public new DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        
        // Customer information
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // Navigation Properties
        public Order? Order { get; set; }
        public Booking? Booking { get; set; }
    }
}
