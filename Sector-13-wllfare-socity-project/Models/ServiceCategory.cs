using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ServiceCategory
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(200)]
        public string? IconClass { get; set; } // For UI icons (e.g., "fas fa-certificate")
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property for related services
        public virtual ICollection<Service>? Services { get; set; }
    }
}


