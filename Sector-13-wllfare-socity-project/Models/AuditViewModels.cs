namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class AuditQuestionnaireViewModel
    {
        public List<AuditQuestion> Questions { get; set; } = new();
        public CompanyCal Booking { get; set; } = new();
        public AuditSession AuditSession { get; set; } = new();
        public List<AuditResponse> Responses { get; set; } = new();
        
        // For creating new questions
        public AuditQuestion NewQuestion { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<Service> Services { get; set; } = new();
    }

    public class AuditResponseViewModel
    {
        public int BookingId { get; set; }
        public List<QuestionResponse> QuestionResponses { get; set; } = new();
        
        public class QuestionResponse
        {
            public int QuestionId { get; set; }
            public string QuestionText { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string ResponseValue { get; set; } = string.Empty; // "Yes", "No", "Partial"
            public string? Comments { get; set; }
        }
    }

    public class CAPViewModel
    {
        public CompanyCal Booking { get; set; } = new();
        public AuditSession AuditSession { get; set; } = new();
        public List<CorrectiveActionPlan> ActionPlans { get; set; } = new();
        
        // Statistics
        public int TotalIssues { get; set; }
        public int CompletedActions { get; set; }
        public int PendingActions { get; set; }
        public int OverdueActions { get; set; }
        
        public DateTime? ExpectedCompletionDate { get; set; }
    }

    public class AuditorDashboardViewModel
    {
        public List<CompanyCal> PendingAudits { get; set; } = new();
        public List<CompanyCal> InProgressAudits { get; set; } = new();
        public List<CompanyCal> CompletedAudits { get; set; } = new();
        
        // Statistics
        public int TotalPendingAudits { get; set; }
        public int TotalCompletedAudits { get; set; }
        public int TotalCAPRequired { get; set; }
        public int TotalAuditsThisMonth { get; set; }
    }

    public class ManagerQuestionnaireViewModel
    {
        public List<AuditQuestion> Questions { get; set; } = new();
        public List<Service> Services { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        
        // For bulk question creation
        public string BulkQuestions { get; set; } = string.Empty; // Textarea for multiple questions
        public string SelectedCategory { get; set; } = string.Empty;
        public int? SelectedServiceId { get; set; }
        
        // Statistics
        public int TotalQuestions { get; set; }
        public int ActiveQuestions { get; set; }
        public Dictionary<string, int> QuestionsByCategory { get; set; } = new();
    }
}