using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class AuditQuestion : Base
    {
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        public int SortOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int? ServiceId { get; set; }  // Optional: link questions to specific services
        public Service? Service { get; set; }
        
        // Navigation properties
        public List<AuditResponse> AuditResponses { get; set; } = new();
    }

    public class AuditResponse : Base
    {
        public int AuditQuestionId { get; set; }
        public AuditQuestion AuditQuestion { get; set; } = null!;
        
        public int BookingId { get; set; }  // CompanyCal ID
        public CompanyCal Booking { get; set; } = null!;
        
        [Required]
        public string ResponseValue { get; set; } = string.Empty; // "Yes", "No", "Partial"
        
        public string? Comments { get; set; }  // Required for "No" and "Partial"
        
        public string? AuditorId { get; set; }  // User who performed the audit
        public ApplicationUser? Auditor { get; set; }
        
        public DateTime AuditDate { get; set; } = DateTime.Now;
    }

    public class AuditSession : Base
    {
        public int BookingId { get; set; }
        public CompanyCal Booking { get; set; } = null!;
        
        public string? AuditorId { get; set; }
        public ApplicationUser? Auditor { get; set; }
        
        public DateTime AuditStartDate { get; set; }
        public DateTime? AuditCompletedDate { get; set; }
        
        public string Status { get; set; } = "In Progress"; // "In Progress", "Completed", "CAP Required"
        
        public bool RequiresCAP { get; set; } = false;
        
        // Navigation properties
        public List<AuditResponse> AuditResponses { get; set; } = new();
        public List<CorrectiveActionPlan> CorrectiveActionPlans { get; set; } = new();
    }

    public class CorrectiveActionPlan : Base
    {
        public int AuditSessionId { get; set; }
        public AuditSession AuditSession { get; set; } = null!;
        
        public int AuditResponseId { get; set; }
        public AuditResponse AuditResponse { get; set; } = null!;
        
        [Required]
        public string IssueDescription { get; set; } = string.Empty;
        
        [Required]
        public string RequiredAction { get; set; } = string.Empty;
        
        public string? MemberResponse { get; set; }
        
        public DateTime DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        
        public string Status { get; set; } = "Pending"; // "Pending", "In Progress", "Completed", "Overdue"
        
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High", "Critical"
    }
}