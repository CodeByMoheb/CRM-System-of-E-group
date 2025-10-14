namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class MailSettings
    {
        public string SmtpServer { get; set; } = "";
        public int Port { get; set; } = 587;
        public string SenderName { get; set; } = "";
        public string SenderEmail { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
        public string AdminEmail { get; set; } = "";
    }
}
