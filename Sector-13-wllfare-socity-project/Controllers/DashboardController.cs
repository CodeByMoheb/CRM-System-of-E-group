using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Data;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            // Redirect to role-specific dashboard
            if (roles.Contains("Admin"))
                return RedirectToAction("Admin");
            else if (roles.Contains("President"))
                return RedirectToAction("President");
            else if (roles.Contains("Secretary"))
                return RedirectToAction("Secretary");
            else if (roles.Contains("Manager"))
                return RedirectToAction("Manager");
            else if (roles.Contains("Member"))
                return RedirectToAction("Member");
            else
                return RedirectToAction("Member"); // Default fallback
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "Admin";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";
            return View();
        }

        [Authorize(Roles = "President")]
        public async Task<IActionResult> President()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "President";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";
            return View();
        }

        [Authorize(Roles = "Secretary")]
        public async Task<IActionResult> Secretary()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "Secretary";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";
            return View();
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Manager()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "Manager";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";

            // Build dashboard model
            var model = new ManagerDashboardViewModel();

            // KPIs
            model.TotalEmployees = await _context.Employees.CountAsync();

            var today = DateTime.Today;
            var todayAttendance = await _context.Attendances
                .Where(a => a.Date == today)
                .ToListAsync();
            model.PresentToday = todayAttendance.Count(a => a.Status != null && a.Status != "Absent");
            model.AbsentToday = todayAttendance.Count(a => a.Status == "Absent");

            model.PendingLeaves = await _context.Leaves.CountAsync(l => l.ApprovalStatus == "Pending");
            model.ActiveShifts = await _context.Shifts.CountAsync();
            model.TotalServices = await _context.Services.CountAsync();
            model.TodayBookings = await _context.Bookings.CountAsync(b => b.BookingDate.Date == today);

            // Recent tables
            model.RecentEmployees = await _context.Employees
                .OrderByDescending(e => e.Id)
                .Take(8)
                .ToListAsync();

            model.RecentLeaveRequests = await _context.Leaves
                .OrderByDescending(l => l.CreatedAt)
                .Take(8)
                .ToListAsync();

            model.RecentBookings = await _context.Bookings
                .OrderByDescending(b => b.BookingDate)
                .Take(8)
                .ToListAsync();

            // Attendance trend for last 7 days
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                model.AttendanceLabels.Add(day.ToString("dd MMM"));
                var dayRecs = await _context.Attendances.Where(a => a.Date == day).ToListAsync();
                model.AttendancePresentCounts.Add(dayRecs.Count(a => a.Status != null && a.Status != "Absent"));
                model.AttendanceAbsentCounts.Add(dayRecs.Count(a => a.Status == "Absent"));
            }

            // Top services by bookings (last 30 days)
            var monthAgo = DateTime.Today.AddDays(-30);
            var serviceCounts = await _context.BookingItems
                .Where(bi => bi.Booking != null && bi.Booking.BookingDate >= monthAgo)
                .GroupBy(bi => bi.ServiceName)
                .Select(g => new { Name = g.Key, Cnt = g.Count() })
                .OrderByDescending(x => x.Cnt)
                .Take(6)
                .ToListAsync();
            model.ServiceLabels = serviceCounts.Select(x => x.Name ?? "Service").ToList();
            model.ServiceUsageCounts = serviceCounts.Select(x => x.Cnt).ToList();

            // Policies and shifts
            model.LeavePolicies = await _context.LeaveEntitlementPolicies
                .Where(p => p.IsActive)
                .OrderBy(p => p.LeaveType)
                .ToListAsync();
            model.Shifts = await _context.Shifts.OrderBy(s => s.Name).ToListAsync();

            return View(model);
        }

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Member()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            var userId = user.UserName;
            
            // Get user's orders count
            var orderCount = await _context.Orders
                .Where(o => o.UserId == userId)
                .CountAsync();
            
            // Get pending orders count
            var pendingOrderCount = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == "Pending")
                .CountAsync();
            
            // Get recent notices count
            var recentNoticesCount = await _context.Notices
                .Where(n => n.IsApproved && n.CreatedAt >= DateTime.Now.AddDays(-30))
                .CountAsync();
            
            // Get upcoming events count (if you have events table, otherwise set to 0)
            var upcomingEventsCount = 0; // Placeholder since no events table found
            
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "Member";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";

            // Dynamic counts for dashboard cards
            ViewBag.OrderCount = orderCount;
            ViewBag.PendingOrderCount = pendingOrderCount;
            ViewBag.RecentNoticesCount = recentNoticesCount;
            ViewBag.UpcomingEventsCount = upcomingEventsCount;

            // Expose whether this identity account is an employee login (EmployeeID as username)
            ViewBag.IsEmployeeUser = !string.IsNullOrWhiteSpace(user.UserName) && user.UserName.StartsWith("EMP", StringComparison.OrdinalIgnoreCase);
            ViewBag.EmployeeId = user.UserName;
            return View();
        }

        // GET: Leave Request Form
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> LeaveRequest()
        {
            System.Diagnostics.Debug.WriteLine($"=== LEAVE REQUEST GET METHOD ===");
            var employeeId = User.Identity?.Name;
            System.Diagnostics.Debug.WriteLine($"Employee ID: {employeeId}");
            
            if (string.IsNullOrEmpty(employeeId))
            {
                System.Diagnostics.Debug.WriteLine("No employee ID, redirecting to login");
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                System.Diagnostics.Debug.WriteLine("Employee not found, redirecting to login");
                return RedirectToAction("Login", "Account");
            }

            System.Diagnostics.Debug.WriteLine($"Employee found: {employee.EmployeeId}, Name: {employee.Name}");

            // Gather active leave policies to drive types and default entitlements
            var activePolicies = await _context.LeaveEntitlementPolicies
                .Where(p => p.IsActive)
                .OrderBy(p => p.LeaveType)
                .ToListAsync();

            ViewBag.LeaveTypes = activePolicies.Select(p => p.LeaveType).ToList();

            var currentYear = DateTime.Now.Year;

            // Fetch existing balances for this employee (current year)
            var existingBalances = await _context.LeaveBalances
                .Where(b => b.EmployeeId == employee.Id && b.Year == currentYear)
                .OrderBy(b => b.LeaveType)
                .ToListAsync();

            // Compute used and pending from actual leaves for correctness
            var thisYearLeaves = await _context.Leaves
                .Where(l => l.EmployeeId == employee.Id && l.StartDate.Year == currentYear)
                .ToListAsync();

            int GetUsed(string type) => thisYearLeaves
                .Where(l => l.LeaveType == type && l.ApprovalStatus == "Approved")
                .Sum(l => l.NumberOfDays);

            int GetPending(string type) => thisYearLeaves
                .Where(l => l.LeaveType == type && l.ApprovalStatus == "Pending")
                .Sum(l => l.NumberOfDays);

            var computedBalances = new List<LeaveBalance>();

            // If there are policies not yet in balances, create display balances from policy defaults
            foreach (var policy in activePolicies)
            {
                var match = existingBalances.FirstOrDefault(b => b.LeaveType == policy.LeaveType);
                var entitled = match?.TotalEntitled ?? policy.DefaultEntitled;
                var used = GetUsed(policy.LeaveType);
                var pending = GetPending(policy.LeaveType);
                var remaining = Math.Max(0, entitled - used - pending);

                computedBalances.Add(new LeaveBalance
                {
                    Id = match?.Id ?? 0,
                    EmployeeId = employee.Id,
                    LeaveType = policy.LeaveType,
                    TotalEntitled = entitled,
                    Used = used,
                    Pending = pending,
                    Remaining = remaining,
                    Year = currentYear
                });
            }

            // Build available balances dictionary for the UI helper
            var available = computedBalances.ToDictionary(b => b.LeaveType, b => b.Remaining);

            var model = new LeaveRequestViewModel
            {
                LeaveType = string.Empty,
                Reason = string.Empty,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                NumberOfDays = 1,
                EmployeeName = employee.Name,
                EmployeeId = employee.EmployeeId,
                LeaveBalances = computedBalances,
                AvailableBalances = available
            };

            ViewBag.Employee = employee;
            return View(model);
        }

        // POST: Submit Leave Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> LeaveRequest(LeaveRequestViewModel model)
        {
            System.Diagnostics.Debug.WriteLine($"=== LEAVE REQUEST SUBMISSION START ===");
            System.Diagnostics.Debug.WriteLine($"Leave request submitted: LeaveType={model?.LeaveType}, StartDate={model?.StartDate}, EndDate={model?.EndDate}, NumberOfDays={model?.NumberOfDays}");
            System.Diagnostics.Debug.WriteLine($"Model is null: {model == null}");
            
            // Log all form data
            System.Diagnostics.Debug.WriteLine($"Request.Form data:");
            foreach (var key in Request.Form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"  {key}: {Request.Form[key]}");
            }
            
            if (model != null)
            {
                System.Diagnostics.Debug.WriteLine($"Model object details:");
                System.Diagnostics.Debug.WriteLine($"- LeaveType: {model.LeaveType}");
                System.Diagnostics.Debug.WriteLine($"- StartDate: {model.StartDate}");
                System.Diagnostics.Debug.WriteLine($"- EndDate: {model.EndDate}");
                System.Diagnostics.Debug.WriteLine($"- NumberOfDays: {model.NumberOfDays}");
                System.Diagnostics.Debug.WriteLine($"- Reason: {model.Reason}");
            }
            
            var employeeId = User.Identity?.Name;
            System.Diagnostics.Debug.WriteLine($"Employee ID from User.Identity.Name: {employeeId}");
            System.Diagnostics.Debug.WriteLine($"User is authenticated: {User.Identity?.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"User claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
            
            // Try to get employee ID from email if User.Identity.Name is not working
            if (string.IsNullOrEmpty(employeeId))
            {
                var userEmail = User.Identity?.Name ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                System.Diagnostics.Debug.WriteLine($"Trying to find employee by email: {userEmail}");
                
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var employeeByEmail = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Email == userEmail);
                    if (employeeByEmail != null)
                    {
                        employeeId = employeeByEmail.EmployeeId;
                        System.Diagnostics.Debug.WriteLine($"Found employee by email: {employeeId}");
                    }
                }
            }

            if (string.IsNullOrEmpty(employeeId))
            {
                System.Diagnostics.Debug.WriteLine("Leave request failed: No employee ID");
                TempData["ErrorMessage"] = "Please login first";
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            System.Diagnostics.Debug.WriteLine($"Employee lookup result: {employee != null}");
            if (employee != null)
            {
                System.Diagnostics.Debug.WriteLine($"Employee found: ID={employee.Id}, EmployeeId={employee.EmployeeId}, Name={employee.Name}");
            }

            if (employee == null)
            {
                System.Diagnostics.Debug.WriteLine("Employee not found in database");
                TempData["ErrorMessage"] = "Employee not found";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Employee found: {employee.EmployeeId}, ID: {employee.Id}");
                
                // Check if the model is valid
                System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("ModelState errors:");
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        if (errors.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"  {key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                    ViewBag.Employee = employee;
                    return View(model);
                }
                
                if (ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("Model validation passed");
                    // Validate dates
                    if (model.StartDate < DateTime.Today)
                    {
                        ModelState.AddModelError("StartDate", "Start date cannot be in the past");
                        ViewBag.Employee = employee;
                        return View(model);
                    }

                    if (model.EndDate < model.StartDate)
                    {
                        ModelState.AddModelError("EndDate", "End date must be after start date");
                        ViewBag.Employee = employee;
                        return View(model);
                    }

                    // Create leave request
                    var leaveRequest = new Leave
                    {
                        EmployeeId = employee.Id,
                        LeaveType = model.LeaveType,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        NumberOfDays = model.NumberOfDays,
                        Reason = model.Reason,
                        ApprovalStatus = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Leaves.Add(leaveRequest);
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine("Leave request saved successfully");
                    TempData["SuccessMessage"] = "Leave request submitted successfully!";
                    return RedirectToAction("Member");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in leave request: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while submitting your leave request. Please try again.";
            }

            ViewBag.Employee = employee;
            return View(model);
        }

        // GET: Leave Balance
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> LeaveBalance()
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get leave balances for the current year
            var currentYear = DateTime.Now.Year;
            var leaveBalances = await _context.LeaveBalances
                .Where(lb => lb.EmployeeId == employee.Id && lb.Year == currentYear)
                .ToListAsync();

            ViewBag.Employee = employee;
            ViewBag.Year = currentYear;
            return View(leaveBalances);
        }

    }
}
