using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sector_13_Welfare_Society___Digital_Management_System.Services;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public ContactController(IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string FullName, string Email, string Message)
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Message))
            {
                TempData["ErrorMessage"] = "❌ Please fill in all required fields.";
                return RedirectToAction("Contact", "Home");
            }

            var subject = $"📩 New Contact Form Message from {FullName}";
            var body = $@"
                <p><strong>Name:</strong> {FullName}</p>
                <p><strong>Email:</strong> {Email}</p>
                <p><strong>Message:</strong></p>
                <p>{Message}</p>
            ";

            // Get AdminEmail from configuration
            var toEmail = _configuration["EmailSettings:AdminEmail"];

            try
            {
                await _emailSender.SendEmailAsync(toEmail, subject, body, Email);

                TempData["SuccessMessage"] = "✅ Your message has been sent successfully!";
            }
            catch
            {
                TempData["ErrorMessage"] = "❌ Sorry, something went wrong while sending your message.";
            }

            return RedirectToAction("ContactUs", "Home");
        }
    }
}
