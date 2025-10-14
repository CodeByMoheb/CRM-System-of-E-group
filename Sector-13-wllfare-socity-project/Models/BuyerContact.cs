using System;
using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class BuyerContact
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string Country { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [Required, StringLength(300)]
        public string Address { get; set; }

        [StringLength(150)]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [OptionalEmail]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(30)]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}



