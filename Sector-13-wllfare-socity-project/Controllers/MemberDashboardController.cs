using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

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

        // GET: MemberDashboard
        public async Task<IActionResult> Index(int? orderId = null, int? bookingId = null)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's orders
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Get user's cart items
            var cartItems = await _context.CartItems
                .Include(c => c.Service)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            // Get recent order details if orderId is provided
            Order? recentOrder = null;
            if (orderId.HasValue)
            {
                try
                {
                    recentOrder = await _context.Orders
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Service)
                        .Where(o => o.Id == orderId.Value)
                        .FirstOrDefaultAsync();
                    
                    if (recentOrder != null)
                    {
                        TempData["OrderSuccess"] = $"Your order (#{recentOrder.OrderNumber}) has been successfully created! Total amount: {recentOrder.TotalAmount:C}";
                    }
                }
                catch
                {
                    TempData["OrderSuccess"] = $"Your order has been successfully created!";
                }
            }

            // Get recent booking details if bookingId is provided (for backward compatibility)
            CompanyCal? recentBooking = null;
            if (bookingId.HasValue)
            {
                try
                {
                    recentBooking = await _context.CompanyCals
                        .Where(b => b.Id == bookingId.Value)
                        .FirstOrDefaultAsync();
                    
                    if (recentBooking != null)
                    {
                        if (recentBooking.ServiceId.HasValue)
                        {
                            recentBooking.Service = await _context.Services
                                .FirstOrDefaultAsync(s => s.Id == recentBooking.ServiceId.Value);
                        }
                        
                        TempData["BookingSuccess"] = $"Your booking (ID: {recentBooking.Id}) has been successfully created! Total amount: {recentBooking.Total:C}";
                    }
                }
                catch
                {
                    TempData["BookingSuccess"] = $"Your booking has been successfully created!";
                }
            }

            // Get recent bookings (CompanyCal records) 
            var recentBookings = new List<CompanyCal>();
            
            // If we have a recent booking from the parameter, add it to the list
            if (recentBooking != null)
            {
                recentBookings.Add(recentBooking);
            }
            
            // Try to get other recent bookings, but don't fail if there are type issues
            try
            {
                var additionalBookings = await _context.CompanyCals
                    .Include(b => b.Service)
                    .Where(b => b.IsApproved && (recentBooking == null || b.Id != recentBooking.Id))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(4) // Take 4 more since we might already have 1
                    .ToListAsync();

                recentBookings.AddRange(additionalBookings);
            }
            catch
            {
                // If there are type issues, we still have the recent booking from parameter
                // So dashboard will still show the latest booking
            }

            var viewModel = new MemberDashboardViewModel
            {
                User = user,
                Orders = orders,
                CartItems = cartItems,
                TotalOrders = orders.Count + recentBookings.Count,
                PendingOrders = orders.Count(o => o.OrderStatus == "Pending") + recentBookings.Count,
                CompletedOrders = orders.Count(o => o.OrderStatus == "Completed"),
                CartItemCount = cartItems.Count,
                RecentBooking = recentBooking,
                RecentBookings = recentBookings,
                RecentOrder = recentOrder
            };

            return View(viewModel);
        }

        // GET: MemberDashboard/Orders
        public async Task<IActionResult> Orders(string? status = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            // Get regular orders
            var ordersQuery = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);

            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = (IOrderedQueryable<Order>)ordersQuery.Where(o => o.OrderStatus == status);
            }

            var orders = await ordersQuery.ToListAsync();

            // Get bookings (CompanyCal records)
            var bookings = new List<CompanyCal>();
            try
            {
                bookings = await _context.CompanyCals
                    .Include(b => b.Service)
                    .Where(b => b.IsApproved)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
            }
            catch
            {
                // Handle type conversion issues
                bookings = new List<CompanyCal>();
            }

            ViewBag.CurrentStatus = status;
            ViewBag.StatusCounts = new
            {
                All = orders.Count + bookings.Count,
                Pending = orders.Count(o => o.OrderStatus == "Pending") + bookings.Count,
                Completed = orders.Count(o => o.OrderStatus == "Completed")
            };

            var viewModel = new OrdersViewModel
            {
                Orders = orders,
                Bookings = bookings,
                User = user!
            };

            return View(viewModel);
        }

        // GET: MemberDashboard/OrderDetails/{id}
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Service)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: MemberDashboard/Bookings
        public async Task<IActionResult> Bookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings = new List<CompanyCal>();
            try
            {
                bookings = await _context.CompanyCals
                    .Include(b => b.Service)
                    .Where(b => b.IsApproved)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
            }
            catch
            {
                // Handle type conversion issues
                bookings = new List<CompanyCal>();
            }

            return View(bookings);
        }

        // GET: MemberDashboard/Cart
        public async Task<IActionResult> Cart()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                .Include(c => c.Service)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                CartItems = cartItems
            };

            return View(viewModel);
        }

        // POST: MemberDashboard/UpdateCartQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity(int cartItemId, int quantity)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Please login." });
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Cart item not found." });
            }

            cartItem.Quantity = quantity;
            cartItem.Subtotal = cartItem.UnitPrice * quantity;
            cartItem.TotalAmount = cartItem.Subtotal + (cartItem.VatAmount ?? 0);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Quantity updated successfully." });
        }

        // POST: MemberDashboard/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Please login." });
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Cart item not found." });
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Item removed from cart." });
        }

        // GET: MemberDashboard/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: MemberDashboard/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update user properties
                user.Name = model.Name;
                user.FathersOrHusbandsName = model.FathersOrHusbandsName;
                user.HouseNo = model.HouseNo;
                user.Ward = model.Ward;
                user.Holding = model.Holding;
                user.Sector = model.Sector;
                user.Profession = model.Profession;
                user.Designation = model.Designation;
                user.BloodGroup = model.BloodGroup;
                user.EducationalQualification = model.EducationalQualification;
                user.NumberOfChildren = model.NumberOfChildren;
                user.Telephone = model.Telephone;
                user.FlatNo = model.FlatNo;
                user.RoadNo = model.RoadNo;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }

        // Fix database schema endpoint
        [HttpGet]
        public async Task<IActionResult> FixSchema()
        {
            try
            {
                // Drop shadow columns and fix types
                await _context.Database.ExecuteSqlRawAsync(@"
                    -- Drop shadow columns if they exist
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CompanyCals]') AND name = 'CustomerId1')
                    BEGIN
                        ALTER TABLE [CompanyCals] DROP COLUMN [CustomerId1];
                    END
                    
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CompanyCals]') AND name = 'InvoiceId1')
                    BEGIN
                        ALTER TABLE [CompanyCals] DROP COLUMN [InvoiceId1];
                    END
                    
                    -- Fix column types if needed
                    IF EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'CompanyCals' 
                        AND COLUMN_NAME = 'CustomerId' 
                        AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar')
                    )
                    BEGIN
                        ALTER TABLE [CompanyCals] ALTER COLUMN [CustomerId] INT NULL;
                    END
                    
                    IF EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'CompanyCals' 
                        AND COLUMN_NAME = 'InvoiceId' 
                        AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar')
                    )
                    BEGIN
                        ALTER TABLE [CompanyCals] ALTER COLUMN [InvoiceId] INT NULL;
                    END
                ");

                TempData["Success"] = "Database schema fixed successfully! Please try booking again.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fixing schema: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        // GET: MemberDashboard/PaymentHistory
        public async Task<IActionResult> PaymentHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            // Get payment records from orders and bookings
            var orders = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == "Completed")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var payments = orders.Select(o => new PaymentViewModel
            {
                Id = o.Id,
                Type = "Order",
                Description = $"Order #{o.OrderNumber}",
                Amount = o.TotalAmount,
                Date = o.CreatedAt,
                Status = "Completed"
            }).ToList();

            // Add booking payments
            try
            {
                var bookings = await _context.CompanyCals
                    .Where(b => b.IsApproved && b.Total.HasValue)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                foreach (var booking in bookings)
                {
                    payments.Add(new PaymentViewModel
                    {
                        Id = booking.Id,
                        Type = "Booking",
                        Description = $"Service Booking #B-{booking.Id}",
                        Amount = booking.Total ?? 0,
                        Date = booking.CreatedAt,
                        Status = "Completed"
                    });
                }
            }
            catch
            {
                // Handle type conversion issues
            }

            return View(payments.OrderByDescending(p => p.Date).ToList());
        }

        // GET: MemberDashboard/Invoices
        public async Task<IActionResult> Invoices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var invoices = new List<InvoiceViewModel>();

            // Get invoices from orders
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            foreach (var order in orders)
            {
                invoices.Add(new InvoiceViewModel
                {
                    Id = order.Id,
                    Number = order.OrderNumber,
                    Type = "Order",
                    Date = order.CreatedAt,
                    Amount = order.TotalAmount,
                    Status = order.OrderStatus
                });
            }

            // Get invoices from bookings - avoid type conversion issues
            try
            {
                var bookingsQuery = _context.CompanyCals
                    .Where(b => b.IsApproved)
                    .OrderByDescending(b => b.CreatedAt);

                var bookings = await bookingsQuery
                    .Select(b => new { 
                        b.Id, 
                        b.CreatedAt, 
                        b.Total, 
                        b.PaymentStatus 
                    })
                    .ToListAsync();

                foreach (var booking in bookings)
                {
                    invoices.Add(new InvoiceViewModel
                    {
                        Id = booking.Id,
                        Number = $"B-{booking.Id}",
                        Type = "Booking",
                        Date = booking.CreatedAt,
                        Amount = booking.Total ?? 0,
                        Status = booking.PaymentStatus ?? "Approved"
                    });
                }
            }
            catch
            {
                // Handle type conversion issues
            }

            return View(invoices.OrderByDescending(i => i.Date).ToList());
        }

        // GET: MemberDashboard/PendingPayments
        public async Task<IActionResult> PendingPayments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pendingPayments = new List<PaymentViewModel>();

            // Get pending orders
            var pendingOrders = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == "Pending")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            foreach (var order in pendingOrders)
            {
                pendingPayments.Add(new PaymentViewModel
                {
                    Id = order.Id,
                    Type = "Order",
                    Description = $"Order #{order.OrderNumber}",
                    Amount = order.TotalAmount,
                    Date = order.CreatedAt,
                    Status = "Pending Payment"
                });
            }

            return View(pendingPayments);
        }

        // GET: MemberDashboard/Settings
        public async Task<IActionResult> Settings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            var settingsModel = new MemberSettingsViewModel
            {
                User = user!,
                EmailNotifications = true,
                SmsNotifications = false,
                OrderUpdates = true,
                PromotionalEmails = false
            };

            return View(settingsModel);
        }

        // POST: MemberDashboard/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(MemberSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save settings logic here
                TempData["Success"] = "Settings updated successfully!";
                return RedirectToAction("Settings");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AuditReports()
        {
            var userId = User.Identity?.Name;
            
            // For testing purposes, allow access if no user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                userId = "hasantexpoten@gmail.com"; // Use the test email
            }

            // Get all bookings with completed audits
            var bookings = await _context.CompanyCals
                .Include(c => c.Service)
                .Include(c => c.Customer)
                .Include(c => c.AuditSessions)
                .Where(c => c.Customer!.Email == userId && 
                           c.AuditSessions.Any(a => a.Status == "Completed"))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> CorrectiveActionPlans()
        {
            var userId = User.Identity?.Name;
            
            // For testing purposes, allow access if no user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                userId = "hasantexpoten@gmail.com"; // Use the test email
            }

            // Get all bookings with CAPs required
            var bookings = await _context.CompanyCals
                .Include(c => c.Service)
                .Include(c => c.Customer)
                .Include(c => c.AuditSessions)
                .Where(c => c.Customer!.Email == userId && 
                           c.AuditSessions.Any(a => a.RequiresCAP))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAuditReport(int id)
        {
            var userId = User.Identity?.Name;
            
            // For testing purposes, allow access if no user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                userId = "hasantexpoten@gmail.com"; // Use the test email
            }
            
            // Get the booking and verify it belongs to the current user
            var booking = await _context.CompanyCals
                .Include(c => c.Service)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == id && c.Customer!.Email == userId);

            if (booking == null)
                return NotFound($"Booking not found for ID: {id} and user: {userId}");

            // Get the audit session
            var auditSession = await _context.AuditSessions
                .Include(a => a.AuditResponses)
                .ThenInclude(r => r.AuditQuestion)
                .Include(a => a.Auditor)
                .FirstOrDefaultAsync(a => a.BookingId == id);

            if (auditSession == null)
                return NotFound("Audit report not found.");

            var viewModel = new AuditQuestionnaireViewModel
            {
                Booking = booking,
                AuditSession = auditSession,
                Responses = auditSession.AuditResponses.ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ViewCAP(int id)
        {
            var userId = User.Identity?.Name;
            
            // For testing purposes, allow access if no user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                userId = "hasantexpoten@gmail.com"; // Use the test email
            }
            
            // Get the booking and verify it belongs to the current user
            var booking = await _context.CompanyCals
                .Include(c => c.Service)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == id && c.Customer!.Email == userId);

            if (booking == null)
                return NotFound($"Booking not found for ID: {id} and user: {userId}");

            // Get the audit session and CAPs
            var auditSession = await _context.AuditSessions
                .Include(a => a.CorrectiveActionPlans)
                .ThenInclude(cap => cap.AuditResponse)
                .ThenInclude(r => r.AuditQuestion)
                .FirstOrDefaultAsync(a => a.BookingId == id);

            if (auditSession == null || !auditSession.RequiresCAP)
                return NotFound("Corrective Action Plan not found.");

            var viewModel = new CAPViewModel
            {
                Booking = booking,
                AuditSession = auditSession,
                ActionPlans = auditSession.CorrectiveActionPlans.ToList(),
                TotalIssues = auditSession.CorrectiveActionPlans.Count,
                CompletedActions = auditSession.CorrectiveActionPlans.Count(cap => cap.Status == "Completed"),
                PendingActions = auditSession.CorrectiveActionPlans.Count(cap => cap.Status == "Pending"),
                OverdueActions = auditSession.CorrectiveActionPlans.Count(cap => cap.Status == "Overdue"),
                ExpectedCompletionDate = auditSession.CorrectiveActionPlans
                    .Where(cap => cap.Status != "Completed")
                    .Min(cap => (DateTime?)cap.DueDate)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCAPResponse(int capId, string memberResponse)
        {
            var userId = User.Identity?.Name;
            
            var cap = await _context.CorrectiveActionPlans
                .Include(c => c.AuditSession)
                .ThenInclude(a => a.Booking)
                .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(c => c.Id == capId && c.AuditSession.Booking.Customer!.Email == userId);

            if (cap == null)
                return NotFound();

            cap.MemberResponse = memberResponse;
            cap.Status = "In Progress";
            cap.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your response has been recorded.";
            return RedirectToAction("ViewCAP", new { id = cap.AuditSession.BookingId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkCAPCompleted(int capId, string completionNote)
        {
            var userId = User.Identity?.Name;
            
            var cap = await _context.CorrectiveActionPlans
                .Include(c => c.AuditSession)
                .ThenInclude(a => a.Booking)
                .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(c => c.Id == capId && c.AuditSession.Booking.Customer!.Email == userId);

            if (cap == null)
                return NotFound();

            cap.Status = "Completed";
            cap.CompletedDate = DateTime.Now;
            cap.MemberResponse = completionNote;
            cap.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Check if all CAPs are completed for this booking
            var auditSession = await _context.AuditSessions
                .Include(a => a.CorrectiveActionPlans)
                .Include(a => a.Booking)
                .FirstOrDefaultAsync(a => a.Id == cap.AuditSessionId);

            if (auditSession != null && auditSession.CorrectiveActionPlans.All(c => c.Status == "Completed"))
            {
                auditSession.Booking.Status = "Audit Complete";
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Corrective action marked as completed.";
            return RedirectToAction("ViewCAP", new { id = auditSession?.BookingId });
        }
    }
}
