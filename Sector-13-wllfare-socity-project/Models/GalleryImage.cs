using System;
using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class GalleryImage:Base
    {
    

     
        public string Title { get; set; } = string.Empty;

        
        public int CategoryId { get; set; } // socialwork, events, specialsday, development, other

     
        public string ImageUrl { get; set; } = string.Empty; // Cloudinary secure_url

        public string? PublicId { get; set; } // Cloudinary public_id (optional with unsigned upload)

        public Category? Category { get; set; }


    }
    public class GalleryViewModel
    {
        public List<GalleryImage> Galleries { get; set; } = new();
        public GalleryImageVm Gallery { get; set; } = new GalleryImageVm();
        public List<Category> Categories { get; set; } = new();

    }
    public class GalleryImageVm:Base
    {
        

        [Required(ErrorMessage ="Enter The Title"), StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }

        public string? PublicId { get; set; }

        
       
    }

}



