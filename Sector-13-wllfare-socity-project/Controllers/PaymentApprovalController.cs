using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class PaymentApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentApprovalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PaymentApproval/Index
        public async Task<IActionResult> Index()
        {
            var pendingPayments = await _context.PaymentRecords
                .Where(p => p.RequiresApproval && p.ApprovalStatus == "Pending")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(pendingPayments);
        }

        // GET: PaymentApproval/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var paymentRecord = await _context.PaymentRecords
                .Include(p => p.Booking)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paymentRecord == null)
            {
                return NotFound();
            }

            return View(paymentRecord);
        }

        // POST: PaymentApproval/Approve/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? notes)
        {
            try
            {
                var paymentRecord = await _context.PaymentRecords.FindAsync(id);
                if (paymentRecord == null)
                {
                    return Json(new { success = false, message = "Payment record not found" });
                }

                // Update payment record
                paymentRecord.Status = "Completed";
                paymentRecord.ApprovalStatus = "Approved";
                paymentRecord.ApprovedBy = User.Identity?.Name ?? "Manager";
                paymentRecord.ApprovedAt = DateTime.Now;
                paymentRecord.VerifiedAt = DateTime.Now;
                paymentRecord.VerifiedBy = User.Identity?.Name ?? "Manager";
                
                if (!string.IsNullOrEmpty(notes))
                {
                    paymentRecord.Notes = (paymentRecord.Notes ?? "") + $"\nManager Notes: {notes}";
                }

                // Update related booking or order
                if (paymentRecord.BookingId.HasValue)
                {
                    var booking = await _context.Bookings.FindAsync(paymentRecord.BookingId);
                    if (booking != null)
                    {
                        booking.PaymentStatus = "Completed";
                        booking.BookingStatus = "Confirmed";
                        booking.PaymentDate = DateTime.Now;
                    }
                }
                else if (paymentRecord.OrderId > 0)
                {
                    var order = await _context.Orders.FindAsync(paymentRecord.OrderId);
                    if (order != null)
                    {
                        order.PaymentStatus = "Completed";
                        order.OrderStatus = "Paid";
                        order.PaymentDate = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Payment approved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: PaymentApproval/Reject/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            try
            {
                var paymentRecord = await _context.PaymentRecords.FindAsync(id);
                if (paymentRecord == null)
                {
                    return Json(new { success = false, message = "Payment record not found" });
                }

                // Update payment record
                paymentRecord.Status = "Rejected";
                paymentRecord.ApprovalStatus = "Rejected";
                paymentRecord.RejectedBy = User.Identity?.Name ?? "Manager";
                paymentRecord.RejectedAt = DateTime.Now;
                paymentRecord.RejectionReason = reason;

                // Update related booking or order
                if (paymentRecord.BookingId.HasValue)
                {
                    var booking = await _context.Bookings.FindAsync(paymentRecord.BookingId);
                    if (booking != null)
                    {
                        booking.PaymentStatus = "Rejected";
                        booking.Notes = (booking.Notes ?? "") + $"\nPayment Rejected: {reason}";
                    }
                }
                else if (paymentRecord.OrderId > 0)
                {
                    var order = await _context.Orders.FindAsync(paymentRecord.OrderId);
                    if (order != null)
                    {
                        order.PaymentStatus = "Rejected";
                        order.Notes = (order.Notes ?? "") + $"\nPayment Rejected: {reason}";
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Payment rejected successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: PaymentApproval/History
        public async Task<IActionResult> History()
        {
            var allPayments = await _context.PaymentRecords
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(allPayments);
        }

        // GET: PaymentApproval/Approve/{id}
        public async Task<IActionResult> Approve(int id)
        {
            var paymentRecord = await _context.PaymentRecords.FindAsync(id);
            if (paymentRecord == null)
            {
                return NotFound();
            }

            return View(paymentRecord);
        }

        // GET: PaymentApproval/Reject/{id}
        public async Task<IActionResult> Reject(int id)
        {
            var paymentRecord = await _context.PaymentRecords.FindAsync(id);
            if (paymentRecord == null)
            {
                return NotFound();
            }

            return View(paymentRecord);
        }
    }
}
