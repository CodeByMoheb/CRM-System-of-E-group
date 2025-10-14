namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class MemberDashboardViewModel
    {
        public ApplicationUser User { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<CartItem> CartItems { get; set; } = new();
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CartItemCount { get; set; }
        public CompanyCal? RecentBooking { get; set; }
        public List<CompanyCal> RecentBookings { get; set; } = new();
        public Order? RecentOrder { get; set; }
        public decimal TotalSpent => Orders.Where(o => o.OrderStatus == "Completed").Sum(o => o.TotalAmount);
    }
}
