namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class MemberSettingsViewModel
    {
        public ApplicationUser User { get; set; } = new();
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool OrderUpdates { get; set; }
        public bool PromotionalEmails { get; set; }
    }
}
