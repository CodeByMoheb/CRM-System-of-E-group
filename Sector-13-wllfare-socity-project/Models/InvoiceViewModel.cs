namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Order" or "Booking"
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Booking/Order related properties
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime? ServiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Customer information
        public string CustomerName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        
        // Payment information
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        
        // Service details
        public List<BookingItem> BookingItems { get; set; } = new();
        public string SpecialRequirements { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        
        // Additional properties for compatibility with existing views
        public Invoice? Invoice { get; set; }
        public Customer? Customer { get; set; }
        public CompanyCal? Booking { get; set; }
    }
}