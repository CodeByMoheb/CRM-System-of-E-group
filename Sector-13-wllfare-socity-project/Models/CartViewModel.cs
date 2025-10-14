namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal => CartItems.Sum(item => item.TotalAmount);
        public decimal Tax { get; set; } = 0;
        public decimal Total => Subtotal + Tax;
        public decimal TotalAmount => Total; // For compatibility
        public int ItemCount => CartItems.Count;
        public ApplicationUser? User { get; set; }
    }
}
