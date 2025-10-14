namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class PaymentViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Order" or "Booking"
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;

        // For payment form
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Currency { get; set; } = "BDT";
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
    }
}
