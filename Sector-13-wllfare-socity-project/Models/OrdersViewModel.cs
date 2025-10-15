namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class OrdersViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public List<Booking> Bookings { get; set; } = new();
        public ApplicationUser User { get; set; } = new();
    }
}
