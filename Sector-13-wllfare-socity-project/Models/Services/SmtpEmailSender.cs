using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public SmtpEmailSender(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings?.Value ?? throw new ArgumentNullException(nameof(mailSettings));
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? replyTo = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("toEmail is required", nameof(toEmail));

            var message = new MimeMessage();

            // From must be the authenticated sender for most SMTP providers (Gmail etc.)
            message.From.Add(new MailboxAddress(_mailSettings.SenderName ?? string.Empty, _mailSettings.SenderEmail ?? string.Empty));

            // To
            message.To.Add(MailboxAddress.Parse(toEmail));

            // Reply-To (optional): set to user's email from contact form
            if (!string.IsNullOrWhiteSpace(replyTo))
            {
                try
                {
                    message.ReplyTo.Add(MailboxAddress.Parse(replyTo));
                }
                catch
                {
                    // If replyTo is invalid, ignore it (avoid throwing from here)
                }
            }

            message.Subject = subject ?? string.Empty;
            message.Body = new TextPart("html") { Text = htmlBody ?? string.Empty };

            using var client = new SmtpClient();
            try
            {
                // Choose socket options based on typical ports
                SecureSocketOptions socketOptions;
                if (_mailSettings.EnableSsl)
                {
                    socketOptions = _mailSettings.Port == 465
                        ? SecureSocketOptions.SslOnConnect
                        : SecureSocketOptions.StartTls;
                }
                else
                {
                    socketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                }

                await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.Port, socketOptions);

                if (!string.IsNullOrWhiteSpace(_mailSettings.Username))
                {
                    await client.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch
            {
                // Consider replacing with ILogger logging in production
                throw;
            }
        }
    }
}
