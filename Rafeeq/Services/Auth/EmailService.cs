







//using Microsoft.Extensions.Configuration;
//using System.Net.Mail;
//using System.Net;
//using System.Threading.Tasks;

//namespace Rafeeq.Services.Auth
//{
//    public class EmailService : IEmailService
//    {
//        private readonly IConfiguration _config;

//        public EmailService(IConfiguration config)
//        {
//            this._config = config;
//        }

//        public async Task SendEmailAsync(string toEmail, string subject, string message)
//        {
//            var emailSettings = _config.GetSection("EmailSettings");
//            var smtpServer = emailSettings["SmtpServer"] ?? throw new InvalidOperationException("SmtpServer is not configured.");
//            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
//            var senderEmail = emailSettings["SenderEmail"] ?? throw new InvalidOperationException("SenderEmail is not configured.");
//            var senderPassword = emailSettings["SenderPassword"] ?? throw new InvalidOperationException("SenderPassword is not configured.");

//            var fromEmailAddress = emailSettings["FromEmailAddress"] ?? throw new InvalidOperationException("FromEmailAddress is not configured.");
//            var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

//            using (var client = new SmtpClient(smtpServer, smtpPort))
//            {
//                client.EnableSsl = enableSsl;
//                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

//                var mailMessage = new MailMessage
//                {
//                    From = new MailAddress(fromEmailAddress, "Rafeeq Platform"),
//                    Subject = subject,
//                    Body = message,
//                    IsBodyHtml = true,
//                };
//                mailMessage.To.Add(toEmail);

//                await client.SendMailAsync(mailMessage);
//            }
//        }

//        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
//        {
//            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
//            var resetLink = $"{frontendUrl}/reset-password?token={token}";
//            //var resetLink = $"{frontendUrl}/reset-password/{token}";

//            var subject = "Rafeeq: Password Reset Request";
//            var message = $"You have requested a password reset. Click the link in you Email: <a href=\"{resetLink}\">{resetLink}</a>";

//            await SendEmailAsync(toEmail, subject, message);
//        }

//        public async Task SendVerificationEmailAsync(string toEmail, string token)
//        {
//            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
//            var verificationLink = $"{frontendUrl}/verify-email/{token}";
//            var subject = "Rafeeq: Verify Your Email Address";
//            var message = $" verify your email by clicking on this link: <a href=\"{verificationLink}\">{verificationLink}</a>";

//            await SendEmailAsync(toEmail, subject, message);
//        }

//        // method for payment confirmation emails
//        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType)
//        {
//            var subject = "Rafeeq: Payment Confirmation";

//            string message;
//            if (userType.ToLower() == "mentor")
//            {
//                message = $@"
//                 <h2>Payment Received</h2>
//                 <p>Dear {userName},</p>
//                 <p>Good news! You've received a payment of ${amount:F2} for booking #{bookingId}.</p>
//                 <p>The session is scheduled for {sessionDateTime.ToString("f")}.</p>
//                 <p>Please log in to your dashboard to view more details.</p>
//                 <p>Thank you for being part of Rafeeq!</p>
//                 ";
//            }
//            else
//            {
//                message = $@"
//                 <h2>Payment Confirmation</h2>
//                 <p>Dear {userName},</p>
//                 <p>Your payment of ${amount:F2} for booking #{bookingId} was successful.</p>
//                 <p>The session is scheduled for {sessionDateTime.ToString("f")}.</p>
//                 <p>Please log in to your dashboard to view more details and join the session.</p>
//                 <p>Thank you for using Rafeeq!</p>
//                 ";
//            }

//            await SendEmailAsync(toEmail, subject, message);
//        }

//    }
//}



﻿
using SendGrid; 
using SendGrid.Helpers.Mail;

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
            var sendGridApiKey = _config["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid API Key is not configured.");

            var fromEmailAddress = _config["EmailSettings:FromEmailAddress"] ?? throw new InvalidOperationException("FromEmailAddress is not configured.");
            var fromName = _config["EmailSettings:FromName"] ?? "Rafeeq Platform";

            var client = new SendGridClient(sendGridApiKey);

            var from = new EmailAddress(fromEmailAddress, fromName);

            var to = new EmailAddress(toEmail);


            var plainTextContent = string.Empty;
            var htmlContent = message;


            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            try
            {
                var response = await client.SendEmailAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    Console.WriteLine($"SendGrid email failed for {toEmail}. Status Code: {response.StatusCode}. Body: {errorBody}");
                    throw new Exception($"SendGrid failed to send email. Status: {response.StatusCode}, Body: {errorBody}");
                }
                Console.WriteLine($"Email sent successfully to {toEmail} via SendGrid. Status Code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending email to {toEmail} via SendGrid: {ex.Message}");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?token={token}";

            var subject = "Rafeeq: Password Reset Request";
            var message = $"You have requested a password reset. Click the link in your Email: <a href=\"{resetLink}\">{resetLink}</a>";

            await SendEmailAsync(toEmail, subject, message);
        }

        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var verificationLink = $"{frontendUrl}/verify-email/{token}";
            var subject = "Rafeeq: Verify Your Email Address";
            var message = $"Verify your email by clicking on this link: <a href=\"{verificationLink}\">{verificationLink}</a>";

            await SendEmailAsync(toEmail, subject, message);
        }


        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType)
        {
            var subject = "Rafeeq: Payment Confirmation";

            string message;
            if (userType.ToLower() == "mentor")
            {
                message = $@"
                    <h2>Payment Received</h2>
                    <p>Dear {userName},</p>
                    <p>Good news! You've received a payment of ${amount:F2} for booking #{bookingId}.</p>
                    <p>The session is scheduled for {sessionDateTime.ToString("f")}.</p>
                    <p>Please log in to your dashboard to view more details.</p>
                    <p>Thank you for being part of Rafeeq!</p>
                    ";

            }
            else
            {
                message = $@"
                    <h2>Payment Confirmation</h2>
                    <p>Dear {userName},</p>
                    <p>Your payment of ${amount:F2} for booking #{bookingId} was successful.</p>
                    <p>The session is scheduled for {sessionDateTime.ToString("f")}.</p>
                    <p>Please log in to your dashboard to view more details and join the session.</p>
                    <p>Thank you for using Rafeeq!</p>
                    ";
            }

            await SendEmailAsync(toEmail, subject, message);
        }
    }
}


