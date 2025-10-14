using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Category: Base
    {


        public string? Type { get; set; }

        public string? Value { get; set; }

        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public int Serial { get; set; }


    }
    public class CategoryViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public CategoryVm Category { get; set; } = new CategoryVm();
    }
    public class CategoryVm : Base
    {

        [Required(ErrorMessage ="Fill the Field"), StringLength(40)]
        public string? Type { get; set; }
        [Required(ErrorMessage = "Fill the Field"), StringLength(40)]
        public string? Value { get; set; }
        [Required(ErrorMessage = "Fill the Field"), StringLength(40)]
        public string? Name { get; set; }
        [Required(ErrorMessage ="Check the active")]
        public bool IsActive { get; set; }
        
        public int Serial { get; set; }


    }
}
