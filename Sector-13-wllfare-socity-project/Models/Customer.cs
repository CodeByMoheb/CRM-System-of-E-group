using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Customer : Base
    {
        public bool IsApproved { get; set; } = true;
        [Required(ErrorMessage = "*Name required.")]
        [StringLength(100)]
        public string? Name { get; set; }
        [Required(ErrorMessage = "*Email required.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "*Phone Number required.")]
        public string? Phone { get; set; }
        public string? HouseNo { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? CompanyName { get; set; }

        public string? CompanyCalId { get; set; }
    }
    public class CustomerVM
    {
        public Customer Customer { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();

    }

}
