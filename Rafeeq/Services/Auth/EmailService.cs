using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace Rafeeq.Services.Auth
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            this._config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"] ?? throw new InvalidOperationException("SmtpServer is not configured.");
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var senderEmail = emailSettings["SenderEmail"] ?? throw new InvalidOperationException("SenderEmail is not configured.");
            var senderPassword = emailSettings["SenderPassword"] ?? throw new InvalidOperationException("SenderPassword is not configured.");

            var fromEmailAddress = emailSettings["FromEmailAddress"] ?? throw new InvalidOperationException("FromEmailAddress is not configured.");
            var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmailAddress, "Rafeeq Platform"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?token={token}";
            //var resetLink = $"{frontendUrl}/reset-password/{token}";

            var subject = "Rafeeq: Password Reset Request";
            var message = $"You have requested a password reset. Please click on this link to reset your password: <a href=\"{resetLink}\">{resetLink}</a>";
            await SendEmailAsync(toEmail, subject, message);
        }

        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var verificationLink = $"{frontendUrl}/verify-email/{token}";
            var subject = "Rafeeq: Verify Your Email Address";
            var message = $"Please verify your email address by clicking on this link: <a href=\"{verificationLink}\">{verificationLink}</a>";
            await SendEmailAsync(toEmail, subject, message);
        }
    }
}