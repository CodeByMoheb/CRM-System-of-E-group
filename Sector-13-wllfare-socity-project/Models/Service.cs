using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string ServiceType { get; set; } = string.Empty; // BSCI, ICS, ISO14001, etc.

        [Required]
        public decimal? Registration_fees { get; set; }
        public decimal? BasePrice { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public bool IsActive { get; set; } = true;



        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public int? ServiceCategoryId { get; set; }
        public ServiceCategory? ServiceCategory { get; set; }
        
        // Display properties
        public int DisplayOrder { get; set; } = 0;

        // Foreign Key

        public string? CompanyCalId { get; set; }
    }

}
