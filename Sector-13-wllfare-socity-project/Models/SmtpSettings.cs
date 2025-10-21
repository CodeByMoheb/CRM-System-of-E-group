namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class SmtpSettingsModel
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
