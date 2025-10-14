using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class BookingListViewModel
    {
        public List<Booking> Bookings { get; set; } = new List<Booking>();
        
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        
        public string Status { get; set; } = "";
        public string Search { get; set; } = "";
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        
        public int PreviousPage => CurrentPage - 1;
        public int NextPage => CurrentPage + 1;
        
        // Additional properties for the view
        public int PageIndex => CurrentPage;
        public int TotalBookings => TotalCount;
        public int ConfirmedBookings => Bookings.Count(b => b.BookingStatus == "Confirmed");
        public int PendingBookings => Bookings.Count(b => b.BookingStatus == "Pending");
    }
}
