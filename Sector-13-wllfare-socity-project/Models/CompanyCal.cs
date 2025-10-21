using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class CompanyCal : Base
    {
        public int? CustomerId { get; set; }
        public decimal? VAT { get; set; }
        public int? LocationChargeId { get; set; }
        public int? ManPowerId { get; set; }
        public decimal? Total { get; set; }
        public decimal? Discount { get; set; }
        public int? InvoiceId { get; set; }
        
    public string? PaymentStatus { get; set; } = "UnPaid";
    public string? Status { get; set; } = "Pending Payment";  // Booking workflow status
    public bool IsApproved { get; set; } = false;
        public int? ServiceId { get; set; }   // Instead of FormType int

        // Navigation 
        public LocationCharge? LocationCharge { get; set; }
        public ManPower? ManPower { get; set; }
        public Customer? Customer { get; set; }
        public Invoice? Invoice { get; set; }
        public Service? Service { get; set; }
        public List<AuditSession> AuditSessions { get; set; } = new();
    }

    public class CompanyCalViewModel
    {
        public CompanyCal CompanyCal { get; set; } = new();

        public List<CompanyCal> CompanyCals { get; set; } = new();
        public List<LocationCharge> LocationCharges { get; set; } = new();
        public List<ManPower> ManPowers { get; set; } = new();
        public List<Service> Services { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();

        public Customer Customer { get; set; } = new();
        public Invoice Invoice { get; set; } = new();
        
        // Booking related properties
        public int? SelectedServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        public string Location { get; set; } = string.Empty;
        public int WorkforceSize { get; set; } = 1;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string SpecialRequirements { get; set; } = string.Empty;
    }


}
