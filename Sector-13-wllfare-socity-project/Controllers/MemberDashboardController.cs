using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Security.Claims;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize]
    public class MemberDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<string> GetUserEmails(ApplicationUser user)
        {
            var userEmails = new List<string>();
            if (!string.IsNullOrEmpty(user.Email)) userEmails.Add(user.Email);
            if (!string.IsNullOrEmpty(user.UserName)) userEmails.Add(user.UserName);
            if (!string.IsNullOrEmpty(user.NormalizedEmail)) userEmails.Add(user.NormalizedEmail);
            if (!string.IsNullOrEmpty(user.NormalizedUserName)) userEmails.Add(user.NormalizedUserName);
            
            return userEmails.Distinct().Where(e => !string.IsNullOrEmpty(e)).ToList();
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
                // Use the same robust email matching logic
                var userEmails = GetUserEmails(user);

                recentBookings = await _context.Bookings
                    .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Service)
                    .Where(b => userEmails.Contains(b.CustomerEmail))
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
                    // Use the same robust email matching logic
                    var userEmails = GetUserEmails(user);

                    recentBooking = await _context.Bookings
                        .Include(b => b.BookingItems)
                        .ThenInclude(bi => bi.Service)
                        .FirstOrDefaultAsync(b => b.Id == bookingId.Value && userEmails.Contains(b.CustomerEmail));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading recent booking: {ex.Message}");
                }
            }

            // Check user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            var isMember = userRoles.Contains("Member");
            var userRole = userRoles.FirstOrDefault() ?? "User";

            var viewModel = new MemberDashboardViewModel
            {
                User = user,
                RecentBooking = recentBooking,
                RecentBookings = recentBookings
            };

            ViewBag.IsMember = isMember;
            ViewBag.UserRole = userRole;

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
                // Debug: Log user information
                Console.WriteLine($"=== MemberDashboard Orders Debug ===");
                Console.WriteLine($"User Email: {user.Email}");
                Console.WriteLine($"User UserName: {user.UserName}");
                Console.WriteLine($"User NormalizedEmail: {user.NormalizedEmail}");
                Console.WriteLine($"User NormalizedUserName: {user.NormalizedUserName}");
                Console.WriteLine($"Status Filter: {status}");

                // More robust email matching - check all possible email variations
                var userEmails = GetUserEmails(user);
                
                Console.WriteLine($"Searching for bookings with emails: {string.Join(", ", userEmails)}");

                // First, let's get ALL bookings to see what's in the database
                var allBookings = await _context.Bookings.Take(10).ToListAsync();
                Console.WriteLine($"Total bookings in database: {allBookings.Count}");
                foreach (var b in allBookings)
                {
                    Console.WriteLine($"All Booking {b.Id}: CustomerEmail='{b.CustomerEmail}', CustomerName='{b.CustomerName}', BookingNumber='{b.BookingNumber}'");
                }

                // Try multiple approaches to find user's bookings
                var bookingsQuery = _context.Bookings
                    .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Service)
                    .Where(b => userEmails.Contains(b.CustomerEmail))
                    .OrderByDescending(b => b.BookingDate);

                // If no bookings found by email, try a broader search
                if (!await bookingsQuery.AnyAsync())
                {
                    Console.WriteLine("No bookings found by email, trying broader search...");
                    
                    // Try case-insensitive search
                    bookingsQuery = _context.Bookings
                        .Include(b => b.BookingItems)
                        .ThenInclude(bi => bi.Service)
                        .Where(b => userEmails.Any(email => 
                            b.CustomerEmail != null && 
                            b.CustomerEmail.ToLower() == email.ToLower()))
                        .OrderByDescending(b => b.BookingDate);
                }

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
                
                // Debug: Log the number of bookings found and their details
                Console.WriteLine($"Found {bookings.Count} bookings for user {user.Email} with status filter: {status}");
                
                // Debug: Log each booking's email for comparison
                foreach (var booking in bookings)
                {
                    Console.WriteLine($"Booking {booking.Id}: CustomerEmail='{booking.CustomerEmail}', CustomerName='{booking.CustomerName}', BookingNumber='{booking.BookingNumber}'");
                }
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

            try
            {
                // Get payment records from PaymentRecord table
                var paymentRecords = await _context.PaymentRecords
                    .Where(p => p.CustomerEmail == user.Email || 
                               p.CustomerEmail == user.UserName || 
                               p.CustomerEmail == user.NormalizedEmail ||
                               p.CustomerEmail == user.NormalizedUserName)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                foreach (var record in paymentRecords)
                {
                    payments.Add(new PaymentViewModel
                    {
                        Id = record.Id,
                        Type = record.BookingId.HasValue ? "Booking" : "Order",
                        Description = record.BookingId.HasValue ? 
                            $"Booking Payment" : 
                            $"Order Payment",
                        Amount = record.Amount,
                        Date = record.CreatedAt,
                        Status = record.Status,
                        OrderId = record.OrderId,
                        OrderNumber = record.TransactionId ?? record.Id.ToString()
                    });
                }
                
                // Debug: Log the number of payments found
                Console.WriteLine($"Found {payments.Count} payment records for user {user.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading payment history: {ex.Message}");
            }

            return View(payments);
        }

        // GET: MemberDashboard/Invoices
        public async Task<IActionResult> Invoices(int page = 1, int pageSize = 9)
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

            // Calculate pagination
            var totalItems = invoices.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedInvoices = invoices.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Create pagination model
            var paginationModel = new PaginationModel<InvoiceViewModel>
            {
                Items = pagedInvoices,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            return View(paginationModel);
        }
    }
}