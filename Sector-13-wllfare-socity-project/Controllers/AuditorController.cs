using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Auditor,Admin")]
    public class AuditorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AuditorDashboardViewModel();
            
            // Get pending audits (paid bookings that need audit)
            viewModel.PendingAudits = await _context.CompanyCals
                .Include(c => c.Service)
                .Include(c => c.Customer)
                .Where(c => c.Status == "Paid" || c.Status == "Processing")
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
            
            // Get in-progress audits
            viewModel.InProgressAudits = await _context.CompanyCals
                .Include(c => c.Service)
                .Include(c => c.Customer)
                .Where(c => c.Status == "Audit In Progress")
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
            
            // Get completed audits
            viewModel.CompletedAudits = await _context.CompanyCals
                .Include(c => c.Service)
                .Include(c => c.Customer)
                .Where(c => c.Status == "Audit Complete" || c.Status == "CAP Required")
                .OrderByDescending(c => c.UpdatedAt)
                .Take(10)
                .ToListAsync();
            
            // Statistics
            viewModel.TotalPendingAudits = viewModel.PendingAudits.Count;
            viewModel.TotalCompletedAudits = await _context.CompanyCals
                .CountAsync(c => c.Status == "Audit Complete");
            viewModel.TotalCAPRequired = await _context.CompanyCals
                .CountAsync(c => c.Status == "CAP Required");
            viewModel.TotalAuditsThisMonth = await _context.CompanyCals
                .CountAsync(c => c.CreatedAt.Month == DateTime.Now.Month && 
                               c.CreatedAt.Year == DateTime.Now.Year &&
                               (c.Status == "Audit Complete" || c.Status == "CAP Required"));

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> StartAudit(int id)
        {
            var booking = await _context.CompanyCals
                .Include(c => c.Service)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (booking == null)
                return NotFound();

            if (booking.Status != "Paid" && booking.Status != "Processing")
                return BadRequest("This booking is not ready for audit.");

            // Get or create audit session
            var auditSession = await _context.AuditSessions
                .Include(a => a.AuditResponses)
                .ThenInclude(r => r.AuditQuestion)
                .FirstOrDefaultAsync(a => a.BookingId == id);

            if (auditSession == null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                auditSession = new AuditSession
                {
                    BookingId = id,
                    AuditorId = currentUser?.Id,
                    AuditStartDate = DateTime.Now,
                    Status = "In Progress"
                };
                _context.AuditSessions.Add(auditSession);
                
                // Update booking status
                booking.Status = "Audit In Progress";
                await _context.SaveChangesAsync();
            }

            // Get all questions for this service or general questions
            var questions = await _context.AuditQuestions
                .Where(q => q.IsActive && (q.ServiceId == null || q.ServiceId == booking.ServiceId))
                .OrderBy(q => q.Category)
                .ThenBy(q => q.SortOrder)
                .ToListAsync();

            var viewModel = new AuditQuestionnaireViewModel
            {
                Booking = booking,
                AuditSession = auditSession,
                Questions = questions,
                Responses = auditSession.AuditResponses.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAuditResponse([FromBody] AuditResponseViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var auditSession = await _context.AuditSessions
                .FirstOrDefaultAsync(a => a.BookingId == model.BookingId);

            if (auditSession == null)
                return NotFound();

            // Remove existing responses for this session
            var existingResponses = _context.AuditResponses
                .Where(r => r.BookingId == model.BookingId);
            _context.AuditResponses.RemoveRange(existingResponses);

            bool requiresCAP = false;

            // Add new responses
            foreach (var response in model.QuestionResponses)
            {
                var auditResponse = new AuditResponse
                {
                    AuditQuestionId = response.QuestionId,
                    BookingId = model.BookingId,
                    ResponseValue = response.ResponseValue,
                    Comments = response.Comments,
                    AuditorId = currentUser?.Id,
                    AuditDate = DateTime.Now
                };

                _context.AuditResponses.Add(auditResponse);

                // Check if CAP is required
                if (response.ResponseValue == "No" || response.ResponseValue == "Partial")
                {
                    requiresCAP = true;
                }
            }

            // Update audit session
            auditSession.AuditCompletedDate = DateTime.Now;
            auditSession.Status = "Completed";
            auditSession.RequiresCAP = requiresCAP;

            // Update booking status
            var booking = await _context.CompanyCals.FindAsync(model.BookingId);
            if (booking != null)
            {
                booking.Status = requiresCAP ? "CAP Required" : "Audit Complete";
            }

            await _context.SaveChangesAsync();

            // If CAP is required, create CAP entries
            if (requiresCAP)
            {
                await CreateCorrectiveActionPlans(auditSession.Id);
            }

            return Json(new { success = true, requiresCAP = requiresCAP });
        }

        private async Task CreateCorrectiveActionPlans(int auditSessionId)
        {
            var auditSession = await _context.AuditSessions
                .Include(a => a.AuditResponses)
                .ThenInclude(r => r.AuditQuestion)
                .FirstOrDefaultAsync(a => a.Id == auditSessionId);

            if (auditSession == null) return;

            var problematicResponses = auditSession.AuditResponses
                .Where(r => r.ResponseValue == "No" || r.ResponseValue == "Partial")
                .ToList();

            foreach (var response in problematicResponses)
            {
                var cap = new CorrectiveActionPlan
                {
                    AuditSessionId = auditSessionId,
                    AuditResponseId = response.Id,
                    IssueDescription = $"{response.AuditQuestion.QuestionText} - Response: {response.ResponseValue}",
                    RequiredAction = response.Comments ?? "Please provide corrective action for this issue.",
                    DueDate = DateTime.Now.AddDays(30), // Default 30 days
                    Status = "Pending",
                    Priority = response.ResponseValue == "No" ? "High" : "Medium"
                };

                _context.CorrectiveActionPlans.Add(cap);
            }

            await _context.SaveChangesAsync();

            // TODO: Send email notification to member
            await SendCAPNotification(auditSession.BookingId);
        }

        private async Task SendCAPNotification(int bookingId)
        {
            // TODO: Implement email notification service
            // This would send an email to the member about the CAP requirements
        }

        [HttpGet]
        public async Task<IActionResult> ViewAuditDetails(int id)
        {
            var auditSession = await _context.AuditSessions
                .Include(a => a.Booking)
                .ThenInclude(b => b.Service)
                .Include(a => a.Booking)
                .ThenInclude(b => b.Customer)
                .Include(a => a.AuditResponses)
                .ThenInclude(r => r.AuditQuestion)
                .Include(a => a.Auditor)
                .FirstOrDefaultAsync(a => a.BookingId == id);

            if (auditSession == null)
                return NotFound();

            return View(auditSession);
        }
    }
}