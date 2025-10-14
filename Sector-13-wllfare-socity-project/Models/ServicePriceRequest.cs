namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ServicePriceRequest
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        public int? WorkforceSize { get; set; }
        public string? Location { get; set; }
        public string? ServiceConfiguration { get; set; }
    }
}


