using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using Microsoft.Data.SqlClient;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ISmsSender _smsSender;
        private readonly IEmailService _emailService;

        public BookingController(ApplicationDbContext context, IConfiguration configuration, ISmsSender smsSender, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _smsSender = smsSender;
            _emailService = emailService;
        }


        // GET: Booking - Single page booking form
        public IActionResult Index()
        {
            var vm = new CompanyCalViewModel
            {
                LocationCharges = _context.LocationCharges.ToList(),
                ManPowers = _context.ManPowers.ToList(),
                Services = _context.Services.Where(s => s.IsActive).ToList(),
                Customers = _context.Customers.ToList(),
                Invoices = _context.Invoices.ToList()
            };

            // Pre-populate customer information for logged-in users
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                    if (user != null)
                    {
                        vm.Customer = new Customer
                        {
                            Name = user.Name ?? user.UserName ?? "",
                            Email = user.Email ?? "",
                            Phone = user.PhoneNumber ?? ""
                        };
                    }
                }
            }

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBooking(CompanyCalViewModel model)
        {
            try
            {
                // Skip ModelState validation for now and do manual validation
                // Initialize objects if null
                if (model.Customer == null)
                    model.Customer = new Customer();
                if (model.CompanyCal == null)
                    model.CompanyCal = new CompanyCal();
                if (model.Invoice == null)
                    model.Invoice = new Invoice();

                // Validate required fields manually
                if (string.IsNullOrWhiteSpace(model.Customer.Name))
                {
                    return Json(new { success = false, message = "Customer name is required." });
                }

                if (string.IsNullOrWhiteSpace(model.Customer.Email))
                {
                    return Json(new { success = false, message = "Customer email is required." });
                }

                if (string.IsNullOrWhiteSpace(model.Customer.Phone))
                {
                    return Json(new { success = false, message = "Customer phone is required." });
                }

                if (model.CompanyCal?.ServiceId == null || model.CompanyCal.ServiceId <= 0)
                {
                    return Json(new { success = false, message = "Please select a service." });
                }

                // If user is not logged in, prompt to login BEFORE any DB writes
                if (!(User?.Identity?.IsAuthenticated ?? false))
                {
                    return Json(new
                    {
                        success = false,
                        requiresLogin = true,
                        message = "You need to be logged in to complete your booking. Please login to continue.",
                        loginUrl = Url.Action("Login", "Account"),
                        registerUrl = Url.Action("Register", "Account")
                    });
                }

                // Ensure required fields have default values if empty
                if (string.IsNullOrWhiteSpace(model.Customer.HouseNo))
                    model.Customer.HouseNo = "N/A";
                if (string.IsNullOrWhiteSpace(model.Customer.Street))
                    model.Customer.Street = "N/A";
                if (string.IsNullOrWhiteSpace(model.Customer.City))
                    model.Customer.City = "N/A";
                if (string.IsNullOrWhiteSpace(model.Customer.CompanyName))
                    model.Customer.CompanyName = "N/A";

                // Set timestamps for customer
                model.Customer.CreatedAt = DateTime.Now;
                model.Customer.ApprovedAt = DateTime.Now;
                model.Customer.IsApproved = true;
                model.Customer.IsDelete = false;

                // Save Customer if new
                if (model.Customer.Id == 0)
                {
                    _context.Customers.Add(model.Customer);
                    await _context.SaveChangesAsync();
                }

                // Calculate totals
                var service = await _context.Services.FindAsync(model.CompanyCal.ServiceId);
                var locationCharge = model.CompanyCal.LocationChargeId.HasValue ? 
                    await _context.LocationCharges.FindAsync(model.CompanyCal.LocationChargeId) : null;
                var manpower = model.CompanyCal.ManPowerId.HasValue ? 
                    await _context.ManPowers.FindAsync(model.CompanyCal.ManPowerId) : null;

                decimal subtotal = 0;
                if (service != null) subtotal += service.Registration_fees ?? 0;
                // Note: Travel allowance (location charge) is added as separate line item, not in subtotal
                if (manpower != null) subtotal += (manpower.ManPowerPrice ?? 0) * (manpower.ManPowerDay ?? 1);

                // Add travel allowance to subtotal for total calculation
                decimal totalBeforeVat = subtotal + (locationCharge?.LChargeValue ?? 0);
                decimal vatAmount = totalBeforeVat * 0.15m; // 15% VAT
                decimal totalAmount = totalBeforeVat + vatAmount;

                // Create Booking record
                var bookingNumber = GenerateBookingNumber();
                
                // Debug: Log the customer email being saved
                Console.WriteLine($"Creating booking with CustomerEmail: '{model.Customer.Email}'");
                Console.WriteLine($"Current logged-in user: {User.Identity?.Name}");
                
                var booking = new Booking
                {
                    BookingNumber = bookingNumber,
                    CustomerName = model.Customer.Name,
                    CustomerEmail = model.Customer.Email ?? "",
                    CustomerPhone = model.Customer.Phone ?? "",
                    CustomerAddress = $"{model.Customer.Street}, {model.Customer.City}",
                    CompanyName = model.Customer.CompanyName,
                    SpecialRequirements = model.SpecialRequirements,
                    TotalAmount = totalAmount,
                    Currency = "BDT",
                    PaymentMethod = "Manual",
                    PaymentStatus = "Pending",
                    BookingStatus = "Pending",
                    BookingDate = DateTime.Now,
                    Notes = "Service booking"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                
                // Debug: Log the saved booking details
                Console.WriteLine($"Booking saved successfully! ID: {booking.Id}, CustomerEmail: '{booking.CustomerEmail}', BookingNumber: '{booking.BookingNumber}'");

                // Create BookingItem for the service
                if (service != null)
                {
                    // Calculate service cost (registration fee + manpower)
                    decimal serviceCost = (service.Registration_fees ?? 0);
                    if (manpower != null) 
                    {
                        serviceCost += (manpower.ManPowerPrice ?? 0) * (manpower.ManPowerDay ?? 1);
                    }

                    var bookingItem = new BookingItem
                    {
                        BookingId = booking.Id,
                        ServiceId = service.Id,
                        ServiceName = service.Name,
                        ServiceDescription = service.Description ?? "",
                        ServiceType = service.ServiceType ?? "",
                        Quantity = 1,
                        UnitPrice = serviceCost, // Service cost only (registration + manpower)
                        Subtotal = serviceCost, // Service cost only
                        VatAmount = vatAmount, // VAT on total amount
                        TotalAmount = totalAmount, // Total amount including VAT
                        Currency = "BDT",
                        Location = model.Customer.City,
                        TravelAllowance = locationCharge?.LChargeValue ?? 0,
                        WorkforceSize = (int?)(manpower?.ManPowerDay ?? 0),
                        ManDaysRequired = (int?)(manpower?.ManPowerDay ?? 0)
                    };

                    _context.BookingItems.Add(bookingItem);
                    await _context.SaveChangesAsync();
                }

                // Create Invoice for the booking (using existing structure)
                var invoice = new Invoice
                {
                    InvoiceId = GenerateInvoiceNumber(),
                    CompanyCalId = booking.Id.ToString(),
                    InvoiceDate = DateTime.Now,
                    IsApproved = false  // Set default value for IsApproved column
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Send email notifications to manager and customer
                await SendBookingNotificationEmails(booking);

                // Store booking details in session
                HttpContext.Session.SetString("BookingId", booking.Id.ToString());
                HttpContext.Session.SetString("CustomerId", model.Customer.Id.ToString());

                // Authenticated path - redirect to dashboard with booking details
                return Json(new { 
                    success = true, 
                    redirectUrl = Url.Action("Index", "MemberDashboard", new { bookingId = booking.Id }),
                    message = "Booking created successfully! You will receive a confirmation email shortly."
                });
            }
            catch (Exception ex)
            {
                // Log the actual exception for debugging
                System.Diagnostics.Debug.WriteLine($"Booking error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Return more detailed error for debugging
                var innerException = ex.InnerException?.Message ?? "";
                var detailedMessage = $"{ex.Message}{(string.IsNullOrEmpty(innerException) ? "" : " Inner: " + innerException)}";
                
                return Json(new { success = false, message = $"Database error: {detailedMessage}" });
            }
        }


        // GET: Booking/AllBookings - Manager view of all bookings
        public async Task<IActionResult> AllBookings(string status = "", string search = "", int page = 1)
        {
            var query = _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.BookingStatus == status);
            }

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => 
                    b.CustomerName.Contains(search) ||
                    b.CustomerEmail.Contains(search) ||
                    b.BookingNumber.Contains(search) ||
                    b.CustomerPhone.Contains(search));
            }

            // Order by booking date descending
            query = query.OrderByDescending(b => b.BookingDate);

            // Pagination
            var pageSize = 10;
            var totalCount = await query.CountAsync();
            var bookings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new BookingListViewModel
            {
                Bookings = bookings,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Status = status,
                Search = search
            };

            return View(viewModel);
        }

        // GET: Booking/CompletedServices - Manager view of completed services
        public async Task<IActionResult> CompletedServices(string search = "", int page = 1)
        {
            var query = _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .Where(b => b.BookingStatus == "Completed")
                .AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => 
                    b.CustomerName.Contains(search) ||
                    b.CustomerEmail.Contains(search) ||
                    b.BookingNumber.Contains(search) ||
                    b.CustomerPhone.Contains(search));
            }

            // Order by service date descending
            query = query.OrderByDescending(b => b.ServiceDate);

            // Pagination
            var pageSize = 10;
            var totalCount = await query.CountAsync();
            var bookings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new BookingListViewModel
            {
                Bookings = bookings,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Search = search
            };

            return View(viewModel);
        }

        // GET: Booking/Invoice/{id} - Get invoice data for modal
        public async Task<IActionResult> Invoice(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return PartialView("_InvoiceModal", booking);
        }

        // POST: Booking/UpdateStatus - Update booking status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                booking.BookingStatus = status;
                
                if (status == "Completed")
                {
                    booking.ServiceDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Send email notifications if status is updated to Completed
                if (status == "Completed")
                {
                    await SendBookingCompletionEmails(booking);
                }

                return Json(new { success = true, message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Booking/CalculatePrice
        [HttpPost]
        public async Task<IActionResult> CalculatePrice([FromBody] ServicePriceRequest request)
        {
            try
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.IsActive);

                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found" });
                }

                var calculation = ServicePricingCalculator.CalculateBookingItem(
                    service,
                    request.Quantity,
                    request.WorkforceSize,
                    request.Location,
                    request.ServiceConfiguration
                );

                return Json(new { success = true, calculation });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Private method to generate booking number
        private string GenerateBookingNumber()
        {
            return $"EG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        // Private method to send booking notification emails
        private async Task SendBookingNotificationEmails(Booking booking)
        {
            try
            {
                // Get manager email from configuration
                var managerEmail = _configuration["EmailSettings:AdminEmail"] ?? "estudioteam.ltd@gmail.com";
                
                // Build absolute logo URL
                var siteBaseUrl = ($"{Request?.Scheme}://{Request?.Host}").TrimEnd('/');
                var logoUrl = siteBaseUrl + "/E-Group.png";

                // Email to customer
                var customerSubject = $"Booking Confirmation - #{booking.BookingNumber}";
                var customerBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; background: white; padding: 30px;'>
                        <meta name='viewport' content='width=device-width, initial-scale=1'>
                        <!-- Invoice Header -->
                        <div style='display: flex; justify-content: space-between; margin-bottom: 30px;'>
                            <div>
                                <div style='display: flex; align-items: center; margin-bottom: 10px;'>
                                    <img src='{logoUrl}' alt='E-Group' style='height:48px;width:auto;margin-right:10px;display:block;'>
                                    <h3 style='color: #007bff; margin: 0; margin-right: 15px;'>E-Group</h3>
                                    <p style='color: #666; margin: 0; font-size: 14px;'>Onestop Textile Ethical Solution</p>
                                </div>
                                <div>
                                    <p style='color: #666; margin: 2px 0; font-size: 12px;'>Sector 13 Welfare Society</p>
                                    <p style='color: #666; margin: 2px 0; font-size: 12px;'>Digital Management System</p>
                                    <p style='color: #666; margin: 2px 0; font-size: 12px;'>Service Provider</p>
                                </div>
                            </div>
                            <div style='text-align: right;'>
                                <h4 style='color: #007bff; font-size: 24px; margin: 0 0 15px 0; letter-spacing: 2px;'>INVOICE</h4>
                                <p style='color: #666; margin: 2px 0; font-size: 12px;'><strong>Invoice #:</strong> {booking.BookingNumber}</p>
                                <p style='color: #666; margin: 2px 0; font-size: 12px;'><strong>Date:</strong> {booking.BookingDate.ToString("dd/MM/yyyy")}</p>
                                <p style='color: #666; margin: 2px 0; font-size: 12px;'><strong>Balance Due:</strong> <span style='color: #007bff; font-weight: bold;'>${booking.TotalAmount:N2}</span></p>
                            </div>
                        </div>

                        <hr style='margin: 20px 0; border: 1px solid #dee2e6;'>

                        <!-- Customer Information -->
                        <div style='display: flex; justify-content: space-between; margin-bottom: 30px;'>
                            <div>
                                <h5 style='color: #333; margin-bottom: 15px; font-weight: bold;'>Bill To:</h5>
                                <p style='margin: 2px 0; font-weight: bold;'>{booking.CustomerName}</p>
                                {(string.IsNullOrEmpty(booking.CompanyName) ? "" : $"<p style='margin: 2px 0; color: #666; font-size: 12px;'>{booking.CompanyName}</p>")}
                                <p style='margin: 2px 0; font-size: 12px;'>{booking.CustomerEmail}</p>
                                <p style='margin: 2px 0; font-size: 12px;'>{booking.CustomerPhone}</p>
                                {(string.IsNullOrEmpty(booking.CustomerAddress) ? "" : $"<p style='margin: 2px 0; color: #666; font-size: 12px;'>{booking.CustomerAddress}</p>")}
                            </div>
                            <div style='text-align: right;'>
                                <h5 style='color: #333; margin-bottom: 15px; font-weight: bold;'>Payment Information:</h5>
                                <p style='margin: 2px 0; font-size: 12px;'><strong>Method:</strong> {booking.PaymentMethod}</p>
                                <p style='margin: 2px 0; font-size: 12px;'><strong>Status:</strong> <span style='background-color: {(booking.PaymentStatus == "Completed" ? "#28a745" : booking.PaymentStatus == "Pending" ? "#ffc107" : "#dc3545")}; color: white; padding: 2px 6px; border-radius: 3px; font-size: 10px;'>{booking.PaymentStatus}</span></p>
                            </div>
                        </div>

                        <!-- Services Table -->
                        <table style='width: 100%; border-collapse: collapse; margin-bottom: 30px;'>
                            <thead>
                                <tr style='background-color: #f8f9fa;'>
                                    <th style='text-align: left; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Item</th>
                                    <th style='text-align: left; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Description</th>
                                    <th style='text-align: right; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Unit Cost</th>
                                    <th style='text-align: center; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Quantity</th>
                                    <th style='text-align: right; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Line Total</th>
                                </tr>
                            </thead>
                            <tbody>";

                foreach (var item in booking.BookingItems)
                {
                    customerBody += $@"
                                <tr>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #007bff; font-weight: bold;'>{item.ServiceName}</td>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #666;'>{(string.IsNullOrEmpty(item.ServiceDescription) ? "-" : item.ServiceDescription)}</td>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>${item.UnitPrice:N2}</td>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: center;'>{item.Quantity}</td>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right; font-weight: bold;'>${item.Subtotal:N2}</td>
                                </tr>";
                    
                    if (item.TravelAllowance.HasValue && item.TravelAllowance > 0)
                    {
                        customerBody += $@"
                                <tr>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #007bff; font-weight: bold;'>Travel Allowance</td>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #666;'>-</td>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>${item.TravelAllowance.Value:N2}</td>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: center;'>1</td>
                                    <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right; font-weight: bold;'>${item.TravelAllowance.Value:N2}</td>
                                </tr>";
                    }
                }

                // Calculate subtotal: service cost + travel allowance
                var serviceSubtotal = booking.BookingItems.Sum(item => item.Subtotal);
                var travelAllowanceTotal = booking.BookingItems.Sum(item => item.TravelAllowance ?? 0);
                var subtotal = serviceSubtotal + travelAllowanceTotal;
                
                // Use the VAT amount from the first BookingItem (which contains the correct VAT)
                var vatAmount = booking.BookingItems.FirstOrDefault()?.VatAmount ?? 0;
                var totalAmount = booking.TotalAmount;

                customerBody += $@"
                            </tbody>
                        </table>

                        <!-- Totals -->
                        <div style='display: flex; justify-content: space-between;'>
                            <div style='flex: 2;'>
                                <p style='color: #666; margin: 0; margin-top: 20px;'>Thanks for your business!</p>
                            </div>
                            <div style='flex: 1;'>
                                <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                                    <tr>
                                        <td style='text-align: left; padding: 8px 0; font-size: 14px;'><strong>Subtotal:</strong></td>
                                        <td style='text-align: right; padding: 8px 0; font-weight: bold; font-size: 14px;'>${subtotal:N2}</td>
                                    </tr>";
                
                if (vatAmount > 0)
                {
                    customerBody += $@"
                                    <tr>
                                        <td style='text-align: left; padding: 8px 0; font-size: 14px;'>VAT (15%):</td>
                                        <td style='text-align: right; padding: 8px 0; font-size: 14px;'>${vatAmount:N2}</td>
                                    </tr>";
                }
                
                customerBody += $@"
                                    <tr>
                                        <td style='text-align: left; padding: 8px 0; font-size: 14px;'>Paid To Date:</td>
                                        <td style='text-align: right; padding: 8px 0; font-size: 14px;'>$0.00</td>
                                    </tr>
                                    <tr style='border-top: 2px solid #007bff; background-color: #f8f9fa;'>
                                        <td style='text-align: left; padding: 12px 0; font-size: 14px;'><strong>Balance Due:</strong></td>
                                        <td style='text-align: right; padding: 12px 0; color: #007bff; font-weight: bold; font-size: 14px;'>${totalAmount:N2}</td>
                                    </tr>
                                </table>
                            </div>
                        </div>

                        <!-- Footer -->
                        <div style='text-align: center; margin-top: 40px;'>
                            <hr style='border-top: 1px solid #dee2e6; margin: 30px 0 20px 0;'>
                            <p style='color: #666; margin: 0; font-size: 12px;'>
                                <strong>Thank you for choosing our services!</strong><br>
                                For any queries, please contact us at e-groupbd@gmail.com
                            </p>
                        </div>
                    </div>
                ";

                // Email to manager
                var managerSubject = $"E-Group - New Booking Received - #{booking.BookingNumber}";
                var managerBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; background: white; padding: 30px;'>
                        <meta name='viewport' content='width=device-width, initial-scale=1'>
                        <h2 style='color: #007bff; text-align: center; margin-bottom: 30px;'>E-Group - New Booking Received</h2>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 30px;'>
                            <h3 style='color: #333; margin-top: 0;'>Booking Summary</h3>
                            <p><strong>Customer:</strong> {booking.CustomerName}</p>
                            <p><strong>Email:</strong> {booking.CustomerEmail}</p>
                            <p><strong>Phone:</strong> {booking.CustomerPhone}</p>
                            <p><strong>Company:</strong> {booking.CompanyName ?? "N/A"}</p>
                            <p><strong>Booking Number:</strong> {booking.BookingNumber}</p>
                            <p><strong>Booking Date:</strong> {booking.BookingDate.ToString("dd/MM/yyyy")}</p>
                            <p><strong>Total Amount:</strong> ${booking.TotalAmount:N2}</p>
                            <p><strong>Status:</strong> {booking.BookingStatus}</p>
                            <p style='margin-top: 15px; font-weight: bold; color: #007bff;'>Please review and update the booking status in the admin panel.</p>
                        </div>

                        <!-- Invoice Details -->
                        <div style='border: 1px solid #dee2e6; border-radius: 5px; padding: 20px;'>
                            <h3 style='color: #333; margin-top: 0; text-align: center;'>Invoice Details</h3>
                            
                            <!-- Services Table -->
                            <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                                <thead>
                                    <tr style='background-color: #f8f9fa;'>
                                        <th style='text-align: left; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Item</th>
                                        <th style='text-align: left; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Description</th>
                                        <th style='text-align: right; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Unit Cost</th>
                                        <th style='text-align: center; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Quantity</th>
                                        <th style='text-align: right; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Line Total</th>
                                    </tr>
                                </thead>
                                <tbody>";

                foreach (var item in booking.BookingItems)
                {
                    managerBody += $@"
                                    <tr>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #007bff; font-weight: bold;'>{item.ServiceName}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #666;'>{(string.IsNullOrEmpty(item.ServiceDescription) ? "-" : item.ServiceDescription)}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>${item.UnitPrice:N2}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: center;'>{item.Quantity}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right; font-weight: bold;'>${item.Subtotal:N2}</td>
                                    </tr>";
                    
                    if (item.TravelAllowance.HasValue && item.TravelAllowance > 0)
                    {
                        managerBody += $@"
                                    <tr>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #007bff; font-weight: bold;'>Travel Allowance</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #666;'>-</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>${item.TravelAllowance.Value:N2}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: center;'>1</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right; font-weight: bold;'>${item.TravelAllowance.Value:N2}</td>
                                    </tr>";
                    }
                }

                managerBody += $@"
                                </tbody>
                            </table>

                            <!-- Totals -->
                            <div style='display: flex; justify-content: flex-end;'>
                                <div style='width: 300px;'>
                                    <table style='width: 100%; border-collapse: collapse;'>
                                        <tr>
                                            <td style='text-align: left; padding: 8px 0; font-size: 14px;'><strong>Subtotal:</strong></td>
                                            <td style='text-align: right; padding: 8px 0; font-weight: bold; font-size: 14px;'>${subtotal:N2}</td>
                                        </tr>";
                
                if (vatAmount > 0)
                {
                    managerBody += $@"
                                        <tr>
                                            <td style='text-align: left; padding: 8px 0; font-size: 14px;'>VAT (15%):</td>
                                            <td style='text-align: right; padding: 8px 0; font-size: 14px;'>${vatAmount:N2}</td>
                                        </tr>";
                }
                
                managerBody += $@"
                                        <tr>
                                            <td style='text-align: left; padding: 8px 0; font-size: 14px;'>Paid To Date:</td>
                                            <td style='text-align: right; padding: 8px 0; font-size: 14px;'>$0.00</td>
                                        </tr>
                                        <tr style='border-top: 2px solid #007bff; background-color: #f8f9fa;'>
                                            <td style='text-align: left; padding: 12px 0; font-size: 14px;'><strong>Balance Due:</strong></td>
                                            <td style='text-align: right; padding: 12px 0; color: #007bff; font-weight: bold; font-size: 14px;'>${totalAmount:N2}</td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>
                ";

                // Send emails
                await _emailService.SendEmailAsync(booking.CustomerEmail, customerSubject, customerBody);
                await _emailService.SendEmailAsync(managerEmail, managerSubject, managerBody);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the booking creation
                System.Diagnostics.Debug.WriteLine($"Email sending error: {ex.Message}");
            }
        }

        // Private method to send booking completion emails
        private async Task SendBookingCompletionEmails(Booking booking)
        {
            try
            {
                // Get manager email from configuration
                var managerEmail = _configuration["EmailSettings:AdminEmail"] ?? "estudioteam.ltd@gmail.com";
                
                // Email to customer
                var customerSubject = $"E-Group - Service Completed - Booking #{booking.BookingNumber}";
                var customerBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; background: white; padding: 30px;'>
                        <meta name='viewport' content='width=device-width, initial-scale=1'>
                        <h2 style='color: #28a745; text-align: center; margin-bottom: 30px;'>E-Group - Service Completed</h2>
                        
                        <div style='background-color: #d4edda; padding: 20px; border-radius: 5px; margin-bottom: 30px; border-left: 4px solid #28a745;'>
                            <h3 style='color: #155724; margin-top: 0;'>Service Completion Confirmation</h3>
                            <p style='color: #155724; margin: 5px 0;'><strong>Dear {booking.CustomerName},</strong></p>
                            <p style='color: #155724; margin: 5px 0;'>Your service has been completed successfully!</p>
                            <p style='color: #155724; margin: 5px 0;'><strong>Booking Number:</strong> {booking.BookingNumber}</p>
                            <p style='color: #155724; margin: 5px 0;'><strong>Service Date:</strong> {booking.ServiceDate?.ToString("dd/MM/yyyy")}</p>
                            <p style='color: #155724; margin: 5px 0;'><strong>Total Amount:</strong> ${booking.TotalAmount:N2}</p>
                        </div>

                        <!-- Invoice Details -->
                        <div style='border: 1px solid #dee2e6; border-radius: 5px; padding: 20px;'>
                            <h3 style='color: #333; margin-top: 0; text-align: center;'>Final Invoice</h3>
                            
                            <!-- Services Table -->
                            <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                                <thead>
                                    <tr style='background-color: #f8f9fa;'>
                                        <th style='text-align: left; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Item</th>
                                        <th style='text-align: left; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Description</th>
                                        <th style='text-align: right; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Unit Cost</th>
                                        <th style='text-align: center; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Quantity</th>
                                        <th style='text-align: right; padding: 12px 8px; border-bottom: 2px solid #dee2e6; font-weight: 600; font-size: 14px;'>Line Total</th>
                                    </tr>
                                </thead>
                                <tbody>";

                foreach (var item in booking.BookingItems)
                {
                    customerBody += $@"
                                    <tr>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #007bff; font-weight: bold;'>{item.ServiceName}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #666;'>{(string.IsNullOrEmpty(item.ServiceDescription) ? "-" : item.ServiceDescription)}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>${item.UnitPrice:N2}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: center;'>{item.Quantity}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right; font-weight: bold;'>${item.Subtotal:N2}</td>
                                    </tr>";
                    
                    if (item.TravelAllowance.HasValue && item.TravelAllowance > 0)
                    {
                        customerBody += $@"
                                    <tr>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #007bff; font-weight: bold;'>Travel Allowance</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; color: #666;'>-</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>${item.TravelAllowance.Value:N2}</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: center;'>1</td>
                                        <td style='padding: 12px 8px; border-bottom: 1px solid #dee2e6; text-align: right; font-weight: bold;'>${item.TravelAllowance.Value:N2}</td>
                                    </tr>";
                    }
                }

                // Calculate subtotal: service cost + travel allowance
                var serviceSubtotal = booking.BookingItems.Sum(item => item.Subtotal);
                var travelAllowanceTotal = booking.BookingItems.Sum(item => item.TravelAllowance ?? 0);
                var subtotal = serviceSubtotal + travelAllowanceTotal;
                
                // Use the VAT amount from the first BookingItem (which contains the correct VAT)
                var vatAmount = booking.BookingItems.FirstOrDefault()?.VatAmount ?? 0;
                var totalAmount = booking.TotalAmount;

                customerBody += $@"
                                </tbody>
                            </table>

                            <!-- Totals -->
                            <div style='display: flex; justify-content: flex-end;'>
                                <div style='width: 300px;'>
                                    <table style='width: 100%; border-collapse: collapse;'>
                                        <tr>
                                            <td style='text-align: left; padding: 8px 0; font-size: 14px;'><strong>Subtotal:</strong></td>
                                            <td style='text-align: right; padding: 8px 0; font-weight: bold; font-size: 14px;'>${subtotal:N2}</td>
                                        </tr>";
                
                if (vatAmount > 0)
                {
                    customerBody += $@"
                                        <tr>
                                            <td style='text-align: left; padding: 8px 0; font-size: 14px;'>VAT (15%):</td>
                                            <td style='text-align: right; padding: 8px 0; font-size: 14px;'>${vatAmount:N2}</td>
                                        </tr>";
                }
                
                customerBody += $@"
                                        <tr>
                                            <td style='text-align: left; padding: 8px 0; font-size: 14px;'>Paid To Date:</td>
                                            <td style='text-align: right; padding: 8px 0; font-size: 14px;'>$0.00</td>
                                        </tr>
                                        <tr style='border-top: 2px solid #007bff; background-color: #f8f9fa;'>
                                            <td style='text-align: left; padding: 12px 0; font-size: 14px;'><strong>Balance Due:</strong></td>
                                            <td style='text-align: right; padding: 12px 0; color: #007bff; font-weight: bold; font-size: 14px;'>${totalAmount:N2}</td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </div>

                        <div style='text-align: center; margin-top: 30px;'>
                            <p style='color: #666; font-size: 16px;'><strong>Thank you for choosing E-Group services!</strong></p>
                            <p style='color: #666; font-size: 14px;'>We hope you are satisfied with our service. Please don't hesitate to contact us for any future needs.</p>
                        </div>
                    </div>
                ";

                // Email to manager
                var managerSubject = $"Service Completed - Booking #{booking.BookingNumber}";
                var managerBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; background: white; padding: 30px;'>
                        <meta name='viewport' content='width=device-width, initial-scale=1'>
                        <h2 style='color:#28a745;text-align:center;margin-bottom:30px;'>Service Completed</h2>
                        <p>A service has been completed:</p>
                        <p><strong>Customer:</strong> {booking.CustomerName}</p>
                        <p><strong>Email:</strong> {booking.CustomerEmail}</p>
                        <p><strong>Phone:</strong> {booking.CustomerPhone}</p>
                        <p><strong>Booking Number:</strong> {booking.BookingNumber}</p>
                        <p><strong>Service Date:</strong> {booking.ServiceDate?.ToString("dd/MM/yyyy")}</p>
                        <p><strong>Total Amount:</strong> ${booking.TotalAmount:N2}</p>
                    </div>
                ";

                // Send emails
                await _emailService.SendEmailAsync(booking.CustomerEmail, customerSubject, customerBody);
                await _emailService.SendEmailAsync(managerEmail, managerSubject, managerBody);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the status update
                System.Diagnostics.Debug.WriteLine($"Email sending error: {ex.Message}");
            }
        }

        //// POST: Booking/ProcessBooking
        //[HttpPost]
        //public async Task<IActionResult> ProcessBooking(BookingFormViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        // Reload services for the form
        //        model.Services = await _context.Services
                  
        //            .Where(s => s.IsActive)
                   
                    
        //            .ToListAsync();
        //        return View("Index", model);
        //    }

        //    try
        //    {
        //        // Get the selected service
        //        var service = await _context.Services
                    
        //            .FirstOrDefaultAsync(s => s.Id == model.SelectedServiceId && s.IsActive);

        //        if (service == null)
        //        {
        //            ModelState.AddModelError("", "Selected service not found");
        //            model.Services = await _context.Services
                     
        //                .Where(s => s.IsActive)
        //                .OrderBy(s => s.ServiceCategory.DisplayOrder)
        //                .ThenBy(s => s.DisplayOrder)
        //                .ToListAsync();
        //            return View("Index", model);
        //        }

        //        // Calculate pricing
        //        var calculation = ServicePricingCalculator.CalculateBookingItem(
        //            service,
        //            model.Quantity,
        //            model.WorkforceSize,
        //            model.Location,
        //            model.ServiceConfiguration
        //        );

        //        // Generate booking number
        //        var bookingNumber = $"EG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        //        // Create booking
        //        var booking = new Booking
        //        {
        //            BookingNumber = bookingNumber,
        //            CustomerName = model.CustomerName,
        //            CustomerEmail = model.CustomerEmail,
        //            CustomerPhone = model.CustomerPhone,
        //            CustomerAddress = model.CustomerAddress,
        //            CompanyName = model.CompanyName,
        //            SpecialRequirements = model.SpecialRequirements,
        //            TotalAmount = calculation.TotalAmount,
        //            Currency = calculation.Currency,
        //            PaymentMethod = "SSLCommerz", // Default to SSLCommerz
        //            PaymentStatus = "Pending",
        //            BookingDate = DateTime.Now,
        //            BookingStatus = "Pending"
        //        };

        //        _context.Bookings.Add(booking);
        //        await _context.SaveChangesAsync();

        //        // Add booking item
        //        var bookingItem = new BookingItem
        //        {
        //            BookingId = booking.Id,
        //            ServiceId = service.Id,
        //            ServiceName = service.Name,
        //            ServiceDescription = service.Description,
        //            ServiceType = service.ServiceType,
        //            Quantity = model.Quantity,
        //            UnitPrice = calculation.UnitPrice,
        //            Subtotal = calculation.Subtotal,
        //            TravelAllowance = calculation.TravelAllowance,
        //            VatAmount = calculation.VatAmount,
        //            TotalAmount = calculation.TotalAmount,
        //            Currency = calculation.Currency,
        //            WorkforceSize = model.WorkforceSize,
        //            ManDaysRequired = calculation.ManDaysRequired,
        //            Location = model.Location,
        //            ServiceConfiguration = model.ServiceConfiguration
        //        };

        //        _context.BookingItems.Add(bookingItem);
        //        await _context.SaveChangesAsync();

        //        // Store booking ID in session for payment
        //        HttpContext.Session.SetString("CurrentBookingId", booking.Id.ToString());

        //        // Send initial booking confirmation email
        //        await SendBookingConfirmationEmail(booking);

        //        // Redirect to payment processing
        //        return RedirectToAction("ProcessPayment", new { id = booking.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", $"An error occurred: {ex.Message}");
        //        model.Services = await _context.Services
        //            .Include(s => s.ServiceCategory)
        //            .Where(s => s.IsActive)
        //            .OrderBy(s => s.ServiceCategory.DisplayOrder)
        //            .ThenBy(s => s.DisplayOrder)
        //            .ToListAsync();
        //        return View("Index", model);
        //    }
        //}

        // GET: Booking/Invoice - Display invoice after booking
        public async Task<IActionResult> InvoiceAfterBooking()
        {
            var bookingIdStr = HttpContext.Session.GetString("BookingId");
            var invoiceIdStr = HttpContext.Session.GetString("InvoiceId");
            var customerIdStr = HttpContext.Session.GetString("CustomerId");

            if (string.IsNullOrEmpty(bookingIdStr) || string.IsNullOrEmpty(invoiceIdStr) || string.IsNullOrEmpty(customerIdStr))
            {
                TempData["Error"] = "No booking found. Please start a new booking.";
                return RedirectToAction("Index");
            }

            var bookingId = int.Parse(bookingIdStr);
            var invoiceId = int.Parse(invoiceIdStr);
            var customerId = int.Parse(customerIdStr);

            var booking = await _context.CompanyCals
                .Include(b => b.Service)
                .Include(b => b.LocationCharge)
                .Include(b => b.ManPower)
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            var customer = await _context.Customers.FindAsync(customerId);
            var invoice = await _context.Invoices.FindAsync(invoiceId);

            if (booking == null || customer == null || invoice == null)
            {
                TempData["Error"] = "Booking details not found.";
                return RedirectToAction("Index");
            }

            var viewModel = new InvoiceViewModel
            {
                Booking = booking,
                Customer = customer,
                Invoice = invoice
            };

            return View(viewModel);
        }

        // GET: Booking/Invoice/{id} - Legacy method for existing bookings
        public async Task<IActionResult> InvoiceById(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return PartialView("_InvoiceModal", booking);
        }

        // POST: Booking/AddToCart - Add booking to cart for logged-in users
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> AddToCart(int bookingId)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Please login to add items to cart." });
            }

            var booking = await _context.CompanyCals
                .Include(b => b.Service)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return Json(new { success = false, message = "Booking not found." });
            }

            // Check if item already exists in cart
            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ServiceId == booking.ServiceId);

            if (existingCartItem != null)
            {
                return Json(new { success = false, message = "This service is already in your cart." });
            }

            // Create cart item
            var cartItem = new CartItem
            {
                UserId = userId,
                ServiceId = booking.ServiceId ?? 0,
                ServiceName = booking.Service?.Name ?? "Service",
                ServiceDescription = booking.Service?.Description ?? "",
                ServiceType = booking.Service?.ServiceType ?? "",
                Quantity = 1,
                UnitPrice = booking.Service?.Registration_fees ?? 0,
                Subtotal = booking.Service?.Registration_fees ?? 0,
                TotalAmount = booking.Total ?? 0,
                Currency = "USD",
                Location = booking.LocationCharge?.LChargeType,
                SpecialRequirements = ""
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Service added to cart successfully!" });
        }

        // GET: Booking/Cart - Display user's cart
        [Microsoft.AspNetCore.Authorization.Authorize]
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

        // POST: Booking/UpdateCartQuantity
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
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

        // POST: Booking/RemoveFromCart
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
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

        // POST: Booking/CreateOrder - Create order from cart
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> CreateOrder()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Your cart is empty." });
            }

            // Create order
            var orderNumber = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                CustomerName = user.Name ?? user.UserName,
                CustomerEmail = user.Email ?? "",
                CustomerPhone = user.PhoneNumber ?? "",
                CustomerAddress = $"{user.HouseNo}, {user.Ward}, {user.Sector}",
                CompanyName = user.Designation,
                Subtotal = cartItems.Sum(c => c.Subtotal),
                VatAmount = cartItems.Sum(c => c.VatAmount ?? 0),
                TotalAmount = cartItems.Sum(c => c.TotalAmount),
                Currency = "USD",
                PaymentMethod = "SSLCommerz",
                PaymentStatus = "Pending",
                OrderStatus = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ServiceId = cartItem.ServiceId,
                    ServiceName = cartItem.ServiceName,
                    ServiceDescription = cartItem.ServiceDescription,
                    ServiceType = cartItem.ServiceType,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    Subtotal = cartItem.Subtotal,
                    TravelAllowance = cartItem.TravelAllowance,
                    VatAmount = cartItem.VatAmount,
                    TotalAmount = cartItem.TotalAmount,
                    Currency = cartItem.Currency,
                    WorkforceSize = cartItem.WorkforceSize,
                    ManDaysRequired = cartItem.ManDaysRequired,
                    Location = cartItem.Location,
                    ServiceConfiguration = cartItem.ServiceConfiguration,
                    SpecialRequirements = cartItem.SpecialRequirements,
                    CreatedAt = DateTime.Now
                };

                _context.OrderItems.Add(orderItem);
            }

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Store order ID in session for payment
            HttpContext.Session.SetString("CurrentOrderId", order.Id.ToString());

            return Json(new { success = true, redirectUrl = Url.Action("ProcessPayment", "Booking") });
        }

        // GET: Booking/ProcessPayment/{id}
        public async Task<IActionResult> ProcessPayment(int? id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Store booking ID in session for payment
            HttpContext.Session.SetString("CurrentBookingId", booking.Id.ToString());

            return View(booking);
        }

        // GET: Booking/ManualPayment/{id}
        [HttpGet]
        public async Task<IActionResult> ManualPayment(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Booking/ManualPayment
        [HttpPost]
        public async Task<IActionResult> ManualPayment(int bookingId, string transactionId, IFormFile paymentReceipt, string paymentNotes)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Save receipt file if provided
                string? savedReceiptPath = null;
                if (paymentReceipt != null && paymentReceipt.Length > 0)
                {
                    var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "payments");
                    if (!Directory.Exists(uploadsRoot))
                    {
                        Directory.CreateDirectory(uploadsRoot);
                    }

                    var fileName = $"receipt_{booking.Id}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(paymentReceipt.FileName)}";
                    var physicalPath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await paymentReceipt.CopyToAsync(stream);
                    }
                    savedReceiptPath = $"/uploads/payments/{fileName}";
                }

                // Update booking with manual payment details
                booking.PaymentMethod = "BankTransfer";
                booking.TransactionId = transactionId;
                booking.PaymentStatus = "Pending Verification";
                booking.PaymentDate = DateTime.Now;

                // Add payment details to notes
                var notesBuilder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(booking.Notes))
                {
                    notesBuilder.AppendLine(booking.Notes);
                }
                notesBuilder.AppendLine($"Manual Payment - Transaction ID: {transactionId}, Submitted: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                if (!string.IsNullOrWhiteSpace(paymentNotes))
                {
                    notesBuilder.AppendLine($"Notes: {paymentNotes}");
                }
                if (!string.IsNullOrEmpty(savedReceiptPath))
                {
                    notesBuilder.AppendLine($"Receipt: {savedReceiptPath}");
                }
                booking.Notes = notesBuilder.ToString();

                await _context.SaveChangesAsync();

                // Create payment record for manual bank transfer
                var paymentRecord = new PaymentRecord
                {
                    BookingId = booking.Id,
                    PaymentMethod = "Bank Transfer",
                    Amount = booking.TotalAmount,
                    Currency = "BDT",
                    Status = "Pending",
                    RequiresApproval = true,
                    ApprovalStatus = "Pending",
                    CustomerName = booking.CustomerName,
                    CustomerEmail = booking.CustomerEmail,
                    CustomerPhone = booking.CustomerPhone ?? "",
                    Notes = "Manual bank transfer - awaiting manager approval",
                    CreatedAt = DateTime.Now
                };
                
                _context.PaymentRecords.Add(paymentRecord);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thank you! Your payment has been submitted and is waiting for approval.";
                return RedirectToAction("BookingSuccess", new { id = booking.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your payment. Please try again.";
                return RedirectToAction("ManualPayment", new { id = bookingId });
            }
        }

        // MANAGER: List pending bank transfer payments
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> PendingBankTransfers()
        {
            var pending = await _context.Bookings
                .Include(b => b.BookingItems)
                .Where(b => b.PaymentMethod == "BankTransfer" && b.PaymentStatus == "Pending Verification")
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            return View(pending);
        }

        // MANAGER: Approve payment
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApprovePayment(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                // Update booking status
                booking.PaymentStatus = "Completed";
                booking.BookingStatus = "Confirmed";
                booking.PaymentDate = DateTime.Now;

                // Update or create payment record
                var paymentRecord = await _context.PaymentRecords
                    .FirstOrDefaultAsync(p => p.BookingId == bookingId);
                
                if (paymentRecord != null)
                {
                    paymentRecord.Status = "Completed";
                    paymentRecord.ApprovalStatus = "Approved";
                    paymentRecord.ApprovedBy = User.Identity?.Name ?? "Manager";
                    paymentRecord.ApprovedAt = DateTime.Now;
                }
                else
                {
                    // Create new payment record for manual approval
                    paymentRecord = new PaymentRecord
                    {
                        BookingId = bookingId,
                        PaymentMethod = "Bank Transfer",
                        Amount = booking.TotalAmount,
                        Currency = "BDT",
                        Status = "Completed",
                        RequiresApproval = true,
                        ApprovalStatus = "Approved",
                        CustomerName = booking.CustomerName,
                        CustomerEmail = booking.CustomerEmail,
                        CustomerPhone = booking.CustomerPhone ?? "",
                        ApprovedBy = User.Identity?.Name ?? "Manager",
                        ApprovedAt = DateTime.Now,
                        VerifiedAt = DateTime.Now,
                        VerifiedBy = User.Identity?.Name ?? "Manager"
                    };
                    _context.PaymentRecords.Add(paymentRecord);
                }

                // Update corresponding invoice (using existing structure)
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.CompanyCalId == booking.Id.ToString());
                
                if (invoice != null)
                {
                    // Update invoice with available properties
                    invoice.InvoiceDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Send completion email
                await SendBookingCompletionEmails(booking);

                return Json(new { success = true, message = "Payment approved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // MANAGER: Review a specific bank transfer
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ReviewBankTransfer(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        // MANAGER: Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveBankTransfer(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            
            booking.PaymentStatus = "Completed";
            booking.PaymentDate = DateTime.Now;
            booking.BookingStatus = "Confirmed";
            
            // Update or create payment record
            var paymentRecord = await _context.PaymentRecords
                .FirstOrDefaultAsync(p => p.BookingId == id);
            
            if (paymentRecord != null)
            {
                paymentRecord.Status = "Completed";
                paymentRecord.ApprovalStatus = "Approved";
                paymentRecord.ApprovedBy = User.Identity?.Name ?? "Manager";
                paymentRecord.ApprovedAt = DateTime.Now;
            }
            else
            {
                // Create new payment record for manual approval
                paymentRecord = new PaymentRecord
                {
                    BookingId = id,
                    PaymentMethod = "Bank Transfer",
                    Amount = booking.TotalAmount,
                    Currency = "BDT",
                    Status = "Completed",
                    RequiresApproval = true,
                    ApprovalStatus = "Approved",
                    CustomerName = booking.CustomerName,
                    CustomerEmail = booking.CustomerEmail,
                    CustomerPhone = booking.CustomerPhone ?? "",
                    ApprovedBy = User.Identity?.Name ?? "Manager",
                    ApprovedAt = DateTime.Now,
                    VerifiedAt = DateTime.Now,
                    VerifiedBy = User.Identity?.Name ?? "Manager"
                };
                _context.PaymentRecords.Add(paymentRecord);
            }
            
            await _context.SaveChangesAsync();
            await NotifyCustomerAsync(booking);
            TempData["Success"] = "Payment approved and booking confirmed.";
            return RedirectToAction(nameof(PendingBankTransfers));
        }

        // MANAGER: Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> RejectBankTransfer(int id, string? reason)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            booking.PaymentStatus = "Rejected";
            var reasonText = string.IsNullOrWhiteSpace(reason) ? "No reason provided" : reason.Trim();
            booking.Notes = (booking.Notes ?? string.Empty) + (string.IsNullOrWhiteSpace(booking.Notes) ? string.Empty : "\n") + $"Manager Rejection: {reasonText}";
            await _context.SaveChangesAsync();
            TempData["Warning"] = "Payment rejected.";
            return RedirectToAction(nameof(PendingBankTransfers));
        }


        // GET: Booking/ProcessSSLCommerz
        public async Task<IActionResult> ProcessSSLCommerz()
        {
            var bookingIdStr = HttpContext.Session.GetString("CurrentBookingId");
            if (string.IsNullOrEmpty(bookingIdStr) || !int.TryParse(bookingIdStr, out int bookingId))
            {
                return RedirectToAction("Index");
            }

            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return RedirectToAction("Index");
            }

            // SSL Commerz configuration
            var storeId = _configuration["SSLCommerz:StoreId"] ?? "";
            var storePassword = _configuration["SSLCommerz:StorePassword"] ?? "";
            var sessionApiUrl = _configuration["SSLCommerz:SessionApiUrl"] ?? "";
            var isSandbox = _configuration["SSLCommerz:IsSandbox"] == "true";
            var storeName = _configuration["SSLCommerz:StoreName"] ?? "E-Group Services";
            var registeredUrl = _configuration["SSLCommerz:RegisteredUrl"] ?? "www.egroup.com";

            if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(storePassword) || string.IsNullOrEmpty(sessionApiUrl))
            {
                TempData["Error"] = "Payment gateway configuration is incomplete. Please contact support.";
                return RedirectToAction(nameof(Index));
            }

            // Generate unique transaction ID
            var tranId = $"TXN_{DateTime.Now:yyyyMMddHHmmss}_{booking.Id}";
            
            // Update booking with transaction ID
            booking.TransactionId = tranId;
            await _context.SaveChangesAsync();

            // Create payment request data
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            
            Console.WriteLine($"=== SSL Payment Request Debug ===");
            Console.WriteLine($"Base URL: {baseUrl}");
            Console.WriteLine($"Registered URL: {registeredUrl}");
            Console.WriteLine($"Store ID: {storeId}");
            Console.WriteLine($"Transaction ID: {tranId}");
            Console.WriteLine($"Total Amount: {booking.TotalAmount:F2}");
            
            var paymentData = new Dictionary<string, string>
            {
                ["store_id"] = storeId,
                ["store_passwd"] = storePassword,
                ["total_amount"] = booking.TotalAmount.ToString("F2"),
                ["currency"] = "BDT",
                ["tran_id"] = tranId,
                ["product_category"] = "Services",
                ["product_name"] = $"Service Booking - {booking.BookingNumber}",
                ["product_profile"] = "general",
                ["cus_name"] = booking.CustomerName ?? "Customer",
                ["cus_email"] = booking.CustomerEmail ?? "customer@example.com",
                ["cus_add1"] = booking.CustomerAddress ?? "Dhaka",
                ["cus_add2"] = "",
                ["cus_city"] = "Dhaka",
                ["cus_state"] = "Dhaka",
                ["cus_postcode"] = "1000",
                ["cus_country"] = "Bangladesh",
                ["cus_phone"] = booking.CustomerPhone ?? "01700000000",
                ["cus_fax"] = "",
                ["ship_name"] = booking.CustomerName ?? "Customer",
                ["ship_add1"] = booking.CustomerAddress ?? "Dhaka",
                ["ship_add2"] = "",
                ["ship_city"] = "Dhaka",
                ["ship_state"] = "Dhaka",
                ["ship_postcode"] = "1000",
                ["ship_country"] = "Bangladesh",
                ["value_a"] = booking.Id.ToString(),
                ["value_b"] = booking.BookingNumber,
                ["value_c"] = booking.CompanyName ?? "",
                ["value_d"] = booking.SpecialRequirements ?? "",
                ["success_url"] = $"{baseUrl}/Booking/PaymentSuccess",
                ["fail_url"] = $"{baseUrl}/Booking/PaymentFail",
                ["cancel_url"] = $"{baseUrl}/Booking/PaymentCancel",
                ["ipn_url"] = $"{baseUrl}/Booking/IPN"
            };
            
            // Log all parameters being sent
            Console.WriteLine("=== SSL Payment Parameters ===");
            foreach (var param in paymentData)
            {
                Console.WriteLine($"{param.Key}: {param.Value}");
            }

            try
            {
                // Call SSL Commerz API to create session
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var content = new FormUrlEncodedContent(paymentData);
                    
                    Console.WriteLine($"=== SSL API Call ===");
                    Console.WriteLine($"URL: {sessionApiUrl}");
                    Console.WriteLine($"Content Type: {content.Headers.ContentType}");
                    
                    var response = await client.PostAsync(sessionApiUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    Console.WriteLine($"=== SSL API Response ===");
                    Console.WriteLine($"Status Code: {response.StatusCode}");
                    Console.WriteLine($"Response Content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var sslResponse = JsonSerializer.Deserialize<SSLCommerzResponse>(responseContent);
                            
                            Console.WriteLine($"=== SSL Response Parsed ===");
                            Console.WriteLine($"Status: {sslResponse?.Status}");
                            Console.WriteLine($"Gateway URL: {sslResponse?.GatewayPageURL}");
                            Console.WriteLine($"Failed Reason: {sslResponse?.FailedReason}");
                            
                            if (sslResponse != null && sslResponse.Status == "SUCCESS" && !string.IsNullOrEmpty(sslResponse.GatewayPageURL))
                            {
                                Console.WriteLine("Redirecting to SSL Gateway...");
                                return Redirect(sslResponse.GatewayPageURL);
                            }
                            else if (sslResponse != null && !string.IsNullOrEmpty(sslResponse.FailedReason))
                            {
                                Console.WriteLine($"SSL Error: {sslResponse.FailedReason}");
                                TempData["Error"] = $"Payment gateway error: {sslResponse.FailedReason}";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                Console.WriteLine("SSL returned invalid response");
                                TempData["Error"] = "Payment gateway returned an invalid response. Please try again.";
                                return RedirectToAction(nameof(Index));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing SSL response: {ex.Message}");
                            TempData["Error"] = $"Error processing payment gateway response: {ex.Message}";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                    else
                    {
                        TempData["Error"] = $"Payment gateway error: HTTP {response.StatusCode}";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to connect to payment gateway. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Booking/PaymentSuccess
        [HttpGet, HttpPost]
        public async Task<IActionResult> PaymentSuccess()
        {
            try
            {
                // Try multiple parameter name variations
                var tranId = Request.Form["tran_id"].FirstOrDefault() 
                    ?? Request.Query["tran_id"].FirstOrDefault()
                    ?? Request.Form["tranId"].FirstOrDefault()
                    ?? Request.Query["tranId"].FirstOrDefault()
                    ?? "";
                
                var status = Request.Form["status"].FirstOrDefault() 
                    ?? Request.Query["status"].FirstOrDefault() 
                    ?? "";
                
                var bankTranId = Request.Form["bank_tran_id"].FirstOrDefault() 
                    ?? Request.Query["bank_tran_id"].FirstOrDefault() 
                    ?? "";
                
                var bookingIdStr = Request.Form["value_a"].FirstOrDefault() 
                    ?? Request.Query["value_a"].FirstOrDefault() 
                    ?? "";
                
                // Enhanced debugging
                Console.WriteLine($"=== SSL Booking Payment Success Callback ===");
                Console.WriteLine($"TranId: '{tranId}'");
                Console.WriteLine($"Status: '{status}'");
                Console.WriteLine($"BankTranId: '{bankTranId}'");
                Console.WriteLine($"BookingId: '{bookingIdStr}'");
                Console.WriteLine($"Request Method: {Request.Method}");
                Console.WriteLine($"Request URL: {Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
                Console.WriteLine($"All Form Keys: {string.Join(", ", Request.Form.Keys)}");
                Console.WriteLine($"Form Data: {string.Join(", ", Request.Form.Select(x => $"{x.Key}={x.Value}"))}");
                Console.WriteLine($"All Query Keys: {string.Join(", ", Request.Query.Keys)}");
                Console.WriteLine($"Query Data: {string.Join(", ", Request.Query.Select(x => $"{x.Key}={x.Value}"))}");
                
                if (string.IsNullOrEmpty(tranId))
                {
                    Console.WriteLine("ERROR: Transaction ID is empty! Redirecting to Index.");
                    TempData["Error"] = "Invalid transaction ID. Payment callback received but transaction ID was missing.";
                    return RedirectToAction(nameof(Index));
                }

                // Find booking
                Booking booking = null;
                
                if (!string.IsNullOrEmpty(bookingIdStr) && int.TryParse(bookingIdStr, out int bookingId))
                {
                    booking = await _context.Bookings.FindAsync(bookingId);
                }
                
                if (booking == null)
                {
                    booking = await _context.Bookings.FirstOrDefaultAsync(b => b.TransactionId == tranId);
                }
                
                if (booking != null)
                {
                    var isSandbox = _configuration["SSLCommerz:IsSandbox"] == "true";
                    var shouldAccept = isSandbox || status == "VALID";
                    
                    if (shouldAccept)
                    {
                        booking.PaymentStatus = "Completed";
                        booking.PaymentDate = DateTime.Now;
                        booking.BookingStatus = "Confirmed";
                        
                        if (!string.IsNullOrEmpty(bankTranId))
                        {
                            booking.TransactionId = bankTranId;
                        }
                        
                        // Create payment record
                        var paymentRecord = new PaymentRecord
                        {
                            BookingId = booking.Id,
                            PaymentMethod = "SSLCommerz",
                            Amount = booking.TotalAmount,
                            Currency = "BDT",
                            TransactionId = bankTranId ?? tranId,
                            Status = "Completed",
                            RequiresApproval = false,
                            ApprovalStatus = "Approved",
                            CustomerName = booking.CustomerName,
                            CustomerEmail = booking.CustomerEmail,
                            CustomerPhone = booking.CustomerPhone ?? "",
                            VerifiedAt = DateTime.Now,
                            VerifiedBy = "System",
                            CreatedAt = DateTime.Now
                        };
                        
                        _context.PaymentRecords.Add(paymentRecord);
                        await _context.SaveChangesAsync();
                        
                        // Send payment confirmation
                        await SendPaymentConfirmationAsync(booking.Id);
                        
                        // Store success data in TempData for the success page
                        TempData["PaymentSuccess"] = "true";
                        TempData["BookingId"] = booking.Id;
                        TempData["BookingNumber"] = booking.BookingNumber;
                        TempData["Amount"] = booking.TotalAmount;
                        TempData["TransactionId"] = bankTranId ?? tranId;
                        TempData["CustomerName"] = booking.CustomerName;
                        
                        return RedirectToAction("PaymentSuccessPage", new { id = booking.Id });
                    }
                    else
                    {
                        booking.PaymentStatus = "Failed";
                        await _context.SaveChangesAsync();
                        
                        TempData["Error"] = "Payment validation failed. Please contact support.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                TempData["Error"] = "Booking not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing payment.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Booking/PaymentFail
        [HttpPost]
        public async Task<IActionResult> PaymentFail()
        {
            try
            {
                var tranId = Request.Form["tran_id"].ToString() ?? "";
                if (string.IsNullOrEmpty(tranId))
                {
                    TempData["Error"] = "Invalid transaction ID.";
                    return RedirectToAction(nameof(Index));
                }

                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.TransactionId == tranId);
                
                if (booking != null)
                {
                    booking.PaymentStatus = "Failed";
                    await _context.SaveChangesAsync();
                    
                    TempData["Error"] = "Payment failed. Please try again.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing payment failure.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Booking/PaymentSuccessPage
        public async Task<IActionResult> PaymentSuccessPage(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // POST: Booking/PaymentCancel
        [HttpPost]
        public async Task<IActionResult> PaymentCancel()
        {
            try
            {
                var tranId = Request.Form["tran_id"].ToString() ?? "";
                if (string.IsNullOrEmpty(tranId))
                {
                    TempData["Warning"] = "Invalid transaction ID.";
                    return RedirectToAction(nameof(Index));
                }

                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.TransactionId == tranId);
                
                if (booking != null)
                {
                    booking.PaymentStatus = "Cancelled";
                    booking.BookingStatus = "Cancelled";
                    await _context.SaveChangesAsync();
                    
                    TempData["Warning"] = "Payment was cancelled.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing payment cancellation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Booking/IPN
        [HttpPost]
        public async Task<IActionResult> IPN()
        {
            try
            {
                var tranId = Request.Form["tran_id"].ToString() ?? "";
                var status = Request.Form["status"].ToString() ?? "";
                var bankTranId = Request.Form["bank_tran_id"].ToString() ?? "";
                var bookingIdStr = Request.Form["value_a"].ToString() ?? "";
                
                if (string.IsNullOrEmpty(tranId))
                {
                    return BadRequest("Invalid transaction ID");
                }

                Booking booking = null;
                
                if (!string.IsNullOrEmpty(bookingIdStr) && int.TryParse(bookingIdStr, out int bookingId))
                {
                    booking = await _context.Bookings.FindAsync(bookingId);
                }
                
                if (booking == null)
                {
                    booking = await _context.Bookings.FirstOrDefaultAsync(b => b.TransactionId == tranId);
                }
                
                if (booking != null)
                {
                    var newStatus = status == "VALID" ? "Completed" : "Failed";
                    
                    try
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        optionsBuilder.UseSqlServer(_context.Database.GetConnectionString());
                        using (var newContext = new ApplicationDbContext(optionsBuilder.Options))
                        {
                            var freshBooking = await newContext.Bookings.FindAsync(booking.Id);
                            if (freshBooking != null)
                            {
                                freshBooking.PaymentStatus = newStatus;
                                if (newStatus == "Completed")
                                {
                                    freshBooking.PaymentDate = DateTime.Now;
                                    freshBooking.BookingStatus = "Confirmed";
                                }
                                
                                if (!string.IsNullOrEmpty(bankTranId))
                                {
                                    freshBooking.TransactionId = bankTranId;
                                }
                                
                                await newContext.SaveChangesAsync();
                                
                                if (freshBooking.PaymentStatus == "Completed")
                                {
                                    // Create payment record for SSL payment
                                    var existingPaymentRecord = await newContext.PaymentRecords
                                        .FirstOrDefaultAsync(pr => pr.BookingId == freshBooking.Id && pr.TransactionId == (bankTranId ?? tranId));
                                    
                                    if (existingPaymentRecord == null)
                                    {
                                        var paymentRecord = new PaymentRecord
                                        {
                                            BookingId = freshBooking.Id,
                                            PaymentMethod = "SSLCommerz",
                                            Amount = freshBooking.TotalAmount,
                                            Currency = "BDT",
                                            TransactionId = bankTranId ?? tranId,
                                            Status = "Completed",
                                            RequiresApproval = false,
                                            ApprovalStatus = "Approved",
                                            CustomerName = freshBooking.CustomerName,
                                            CustomerEmail = freshBooking.CustomerEmail,
                                            CustomerPhone = freshBooking.CustomerPhone ?? "",
                                            VerifiedAt = DateTime.Now,
                                            VerifiedBy = "System",
                                            CreatedAt = DateTime.Now
                                        };
                                        
                                        newContext.PaymentRecords.Add(paymentRecord);
                                        await newContext.SaveChangesAsync();
                                    }
                                    
                                    await NotifyCustomerAsync(freshBooking);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fallback: try direct SQL update
                        await UpdateBookingStatusDirectly(booking.Id, newStatus, bankTranId);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        // GET: Booking/BookingSuccess/5
        public async Task<IActionResult> BookingSuccess(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        private async Task UpdateBookingStatusDirectly(int bookingId, string status, string? bankTranId = null)
        {
            try
            {
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        if (!string.IsNullOrEmpty(bankTranId))
                        {
                            command.CommandText = "UPDATE Bookings SET PaymentStatus = @status, TransactionId = @bankTranId, PaymentDate = @paymentDate, BookingStatus = @bookingStatus WHERE Id = @bookingId";
                            command.Parameters.Add(new SqlParameter("@bankTranId", bankTranId));
                            command.Parameters.Add(new SqlParameter("@paymentDate", DateTime.Now));
                            command.Parameters.Add(new SqlParameter("@bookingStatus", status == "Completed" ? "Confirmed" : "Pending"));
                        }
                        else
                        {
                            command.CommandText = "UPDATE Bookings SET PaymentStatus = @status WHERE Id = @bookingId";
                        }
                        
                        command.Parameters.Add(new SqlParameter("@status", status));
                        command.Parameters.Add(new SqlParameter("@bookingId", bookingId));
                        
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating booking status: {ex.Message}");
            }
        }

        private async Task NotifyCustomerAsync(Booking booking)
        {
            try
            {
                if (booking.PaymentStatus != "Completed")
                {
                    return;
                }

                // SMS
                if (!string.IsNullOrWhiteSpace(booking.CustomerPhone))
                {
                    var sms = $"Thank you, {booking.CustomerName}, for booking our services. Booking No: {booking.BookingNumber}. Total: {booking.Currency} {booking.TotalAmount:F2}. - E-Group";
                    await _smsSender.SendAsync(booking.CustomerPhone, sms);
                }

                // Email with detailed invoice
                if (!string.IsNullOrWhiteSpace(booking.CustomerEmail))
                {
                    await SendBookingConfirmationEmail(booking);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }

        private async Task SendBookingConfirmationEmail(Booking booking)
        {
            try
            {
                var subject = $"Booking Confirmation - {booking.BookingNumber}";
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #2c3e50;'>E-Group</h1>
                            <h2 style='color: #3498db;'>Booking Confirmation</h2>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 20px;'>
                            <h3 style='color: #2c3e50; margin-top: 0;'>Booking Details</h3>
                            <p><strong>Booking Number:</strong> {booking.BookingNumber}</p>
                            <p><strong>Customer Name:</strong> {booking.CustomerName}</p>
                            <p><strong>Company:</strong> {booking.CompanyName}</p>
                            <p><strong>Email:</strong> {booking.CustomerEmail}</p>
                            <p><strong>Phone:</strong> {booking.CustomerPhone}</p>
                            <p><strong>Booking Date:</strong> {booking.BookingDate:dd MMMM yyyy}</p>
                        </div>

                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 20px;'>
                            <h3 style='color: #2c3e50; margin-top: 0;'>Service Details</h3>";

                foreach (var item in booking.BookingItems)
                {
                    body += $@"
                        <div style='border-bottom: 1px solid #dee2e6; padding: 10px 0;'>
                            <h4 style='color: #3498db; margin: 0;'>{item.ServiceName}</h4>
                            <p style='margin: 5px 0;'>{item.ServiceDescription}</p>
                            <p><strong>Quantity:</strong> {item.Quantity}</p>
                            <p><strong>Unit Price:</strong> {item.Currency} {item.UnitPrice:N2}</p>
                            <p><strong>Subtotal:</strong> {item.Currency} {item.Subtotal:N2}</p>";
                    
                    if (item.TravelAllowance.HasValue && item.TravelAllowance > 0)
                    {
                        body += $@"<p><strong>Travel Allowance:</strong> {item.Currency} {item.TravelAllowance:N2}</p>";
                    }
                    
                    if (item.VatAmount.HasValue && item.VatAmount > 0)
                    {
                        body += $@"<p><strong>VAT:</strong> {item.Currency} {item.VatAmount:N2}</p>";
                    }
                    
                    body += $@"<p><strong>Total:</strong> {item.Currency} {item.TotalAmount:N2}</p>
                        </div>";
                }

                body += $@"
                        </div>

                        <div style='background-color: #e8f5e8; padding: 20px; border-radius: 5px; text-align: center;'>
                            <h3 style='color: #27ae60; margin-top: 0;'>Total Amount</h3>
                            <h2 style='color: #27ae60; margin: 0;'>{booking.Currency} {booking.TotalAmount:N2}</h2>
                            <p style='color: #27ae60;'><strong>Payment Status: {booking.PaymentStatus}</strong></p>
                        </div>

                        <div style='margin-top: 30px; text-align: center; color: #7f8c8d;'>
                            <p>Thank you for choosing E-Group services!</p>
                            <p>We will contact you soon to schedule your service.</p>
                            <p>Best regards,<br><strong>E-Group Team</strong></p>
                        </div>
                    </div>
                ";

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the booking
                Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
            }
        }

        private async Task SendPaymentConfirmationAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.BookingItems)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null) return;

                var invoiceUrl = Url.Action("Invoice", "Booking", new { id = booking.Id }, Request.Scheme);
                
                // Send Email
                var emailBody = $@"
                    <h2>Payment Confirmation - {booking.BookingNumber}</h2>
                    <p>Dear {booking.CustomerName},</p>
                    <p>Thank you for your payment. We have received your payment proof and will verify it within 24 hours.</p>
                    <p><strong>Booking Number:</strong> {booking.BookingNumber}</p>
                    <p><strong>Transaction ID:</strong> {booking.TransactionId}</p>
                    <p><strong>Amount Paid:</strong> {booking.Currency} {booking.TotalAmount:N2}</p>
                    <p>You can view your invoice here: <a href='{invoiceUrl}'>View Invoice</a></p>
                    <p>We will notify you once payment verification is complete.</p>
                    <p>Best regards,<br>E-Group Team</p>
                ";

                await _emailService.SendEmailAsync(
                    booking.CustomerEmail,
                    $"Payment Confirmation - {booking.BookingNumber}",
                    emailBody
                );

                // Send SMS
                var smsMessage = $"E-Group: Payment received for booking {booking.BookingNumber}. Amount: {booking.Currency} {booking.TotalAmount:N2}. We will verify and confirm within 24 hours.";
                await _smsSender.SendAsync(booking.CustomerPhone, smsMessage);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error sending payment confirmation: {ex.Message}");
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}