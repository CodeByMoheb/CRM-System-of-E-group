using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Security.Claims;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MemberDashboard/Index
        public async Task<IActionResult> Index(int? orderId = null, int? bookingId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            // Get recent bookings
            var recentBookings = new List<Booking>();
            try
            {
                recentBookings = await _context.Bookings
                    .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Service)
                    .Where(b => b.CustomerEmail == user.Email || 
                               b.CustomerEmail == user.UserName || 
                               b.CustomerEmail == user.NormalizedEmail ||
                               b.CustomerEmail == user.NormalizedUserName)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                .ToListAsync();

                // Debug: Log the number of bookings found
                Console.WriteLine($"Found {recentBookings.Count} recent bookings for user {user.Email} (UserName: {user.UserName})");
                
                // Additional debugging - check all bookings in database
                var allBookings = await _context.Bookings.Take(5).ToListAsync();
                Console.WriteLine($"Total bookings in database: {allBookings.Count}");
                foreach (var booking in allBookings)
                {
                    Console.WriteLine($"Booking {booking.Id}: CustomerEmail='{booking.CustomerEmail}', CustomerName='{booking.CustomerName}'");
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the page
                Console.WriteLine($"Error loading recent bookings: {ex.Message}");
            }

            // Get recent booking (single booking for display)
            Booking? recentBooking = null;
            if (bookingId.HasValue)
            {
                try
                {
                    recentBooking = await _context.Bookings
                        .Include(b => b.BookingItems)
                        .ThenInclude(bi => bi.Service)
                        .FirstOrDefaultAsync(b => b.Id == bookingId.Value && (b.CustomerEmail == user.Email || b.CustomerEmail == user.UserName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading recent booking: {ex.Message}");
                }
            }

            var viewModel = new MemberDashboardViewModel
            {
                User = user,
                RecentBooking = recentBooking,
                RecentBookings = recentBookings
            };

            return View(viewModel);
        }

        // GET: MemberDashboard/Orders
        public async Task<IActionResult> Orders(string? status = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            // Get bookings (since we're using Booking model instead of Orders)
            var bookings = new List<Booking>();
            try
            {
                var bookingsQuery = _context.Bookings
                    .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Service)
                    .Where(b => b.CustomerEmail == user.Email || 
                               b.CustomerEmail == user.UserName || 
                               b.CustomerEmail == user.NormalizedEmail ||
                               b.CustomerEmail == user.NormalizedUserName)
                    .OrderByDescending(b => b.BookingDate);

                if (!string.IsNullOrEmpty(status))
                {
                    if (status == "Pending")
                    {
                        bookingsQuery = (IOrderedQueryable<Booking>)bookingsQuery.Where(b => b.PaymentStatus == "Pending" || b.PaymentStatus == "Pending Verification");
                    }
                    else if (status == "Completed")
                    {
                        bookingsQuery = (IOrderedQueryable<Booking>)bookingsQuery.Where(b => b.PaymentStatus == "Completed");
                    }
                }

                bookings = await bookingsQuery.ToListAsync();
                
                // Debug: Log the number of bookings found
                Console.WriteLine($"Found {bookings.Count} bookings for user {user.Email} with status filter: {status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading bookings: {ex.Message}");
            }

            var viewModel = new OrdersViewModel
            {
                Bookings = bookings
            };

            return View(viewModel);
        }

        // GET: MemberDashboard/PaymentHistory
        public async Task<IActionResult> PaymentHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var payments = new List<PaymentViewModel>();

            // Get payment records from completed bookings
            try
            {
                var completedBookings = await _context.Bookings
                    .Where(b => (b.CustomerEmail == user.Email || 
                                b.CustomerEmail == user.UserName || 
                                b.CustomerEmail == user.NormalizedEmail ||
                                b.CustomerEmail == user.NormalizedUserName) && 
                               b.PaymentStatus == "Completed")
                    .OrderByDescending(b => b.PaymentDate)
                    .ToListAsync();

                foreach (var booking in completedBookings)
                {
                    payments.Add(new PaymentViewModel
                    {
                        Id = booking.Id,
                        Type = "Booking",
                        Description = $"Booking #{booking.BookingNumber}",
                        Amount = booking.TotalAmount,
                        Date = booking.PaymentDate ?? booking.BookingDate,
                        Status = "Completed"
                    });
                }
                
                // Debug: Log the number of payments found
                Console.WriteLine($"Found {payments.Count} completed payments for user {user.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading payment history: {ex.Message}");
            }

            return View(payments);
        }

        // GET: MemberDashboard/Invoices
        public async Task<IActionResult> Invoices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var invoices = new List<InvoiceViewModel>();

            // Get invoices from bookings (since Invoice table has limited structure)
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Service)
                    .Where(b => b.CustomerEmail == user.Email || 
                               b.CustomerEmail == user.UserName || 
                               b.CustomerEmail == user.NormalizedEmail ||
                               b.CustomerEmail == user.NormalizedUserName)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                foreach (var booking in bookings)
                {
                    invoices.Add(new InvoiceViewModel
                    {
                        Id = booking.Id,
                        Number = booking.BookingNumber,
                        Type = "Booking",
                        Date = booking.BookingDate,
                        Amount = booking.TotalAmount,
                        Status = booking.PaymentStatus,
                        BookingNumber = booking.BookingNumber,
                        BookingDate = booking.BookingDate,
                        TotalAmount = booking.TotalAmount,
                        CustomerName = booking.CustomerName,
                        CompanyName = booking.CompanyName ?? "",
                        CustomerEmail = booking.CustomerEmail,
                        CustomerPhone = booking.CustomerPhone ?? "",
                        CustomerAddress = booking.CustomerAddress ?? "",
                        PaymentMethod = booking.PaymentMethod,
                        PaymentStatus = booking.PaymentStatus,
                        TransactionId = booking.TransactionId ?? "",
                        SpecialRequirements = booking.SpecialRequirements ?? "",
                        Notes = booking.Notes ?? "",
                        BookingItems = booking.BookingItems?.ToList() ?? new List<BookingItem>()
                    });
                }
                
                // Debug: Log the number of invoices found
                Console.WriteLine($"Found {invoices.Count} invoices for user {user.Email}");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the page
                Console.WriteLine($"Error loading invoices: {ex.Message}");
            }

            return View(invoices);
        }
    }
}