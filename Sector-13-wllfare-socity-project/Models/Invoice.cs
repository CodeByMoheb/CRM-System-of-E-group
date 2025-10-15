namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Invoice : Base
    {
        public string? InvoiceId { get; set; }
        public string? CompanyCalId { get; set; }
        public DateTime? InvoiceDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;
    }
}
