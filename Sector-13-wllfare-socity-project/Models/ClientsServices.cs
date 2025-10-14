using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ClientsServices:Base
    {
         [Key]
        public new int Id { get; set; }
        public string? Title { get; set; }

        [Display(Name = "Upload Image")]
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }

        public string? Description { get; set; }

        // Explicit approval flag for client services; DB column exists and is non-nullable
        [Display(Name = "Approved")]
        public bool IsApproved { get; set; } = false;

    }
}
