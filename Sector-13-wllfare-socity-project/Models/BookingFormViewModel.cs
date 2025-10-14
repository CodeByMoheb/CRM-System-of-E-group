using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class BookingFormViewModel
    {
        // Service Selection
        [Required(ErrorMessage = "Please select a service")]
        [Display(Name = "Service")]
        public int SelectedServiceId { get; set; }

        public List<Service> Services { get; set; } = new List<Service>();

        // Service Configuration
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Workforce Size")]
        [Range(1, 10000, ErrorMessage = "Workforce size must be between 1 and 10,000")]
        public int WorkforceSize { get; set; } = 100;

        [Display(Name = "Location")]
        [Required(ErrorMessage = "Please select location")]
        public string Location { get; set; } = "Inside Dhaka";

        [Display(Name = "Special Requirements")]
        [StringLength(500, ErrorMessage = "Special requirements cannot exceed 500 characters")]
        public string? SpecialRequirements { get; set; }

        // Customer Information
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Address")]
        public string? CustomerAddress { get; set; }

        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        // Service Configuration (JSON)
        public string? ServiceConfiguration { get; set; }

        // Calculated Fields (read-only)
        public decimal? CalculatedPrice { get; set; }
        public string? Currency { get; set; } = "USD";
        public int? ManDaysRequired { get; set; }
        public decimal? TravelAllowance { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
