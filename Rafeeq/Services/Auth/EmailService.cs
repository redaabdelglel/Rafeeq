
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
            _config = config;
        }


        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            
            var sendGridApiKey = _config["SendGrid:ApiKey"]; 

            var fromEmailAddress = _config["EmailSettings:FromEmailAddress"];
            var fromName = _config["EmailSettings:FromName"] ?? "Rafeeq Platform"; 

            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                throw new InvalidOperationException("SendGrid API Key is not configured.");
            }
            if (string.IsNullOrEmpty(fromEmailAddress))
            {
                throw new InvalidOperationException("FromEmailAddress is not configured in EmailSettings.");
            }

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
                    Console.WriteLine($"SendGrid Email sending failed with status code: {response.StatusCode}");
                    var errorBody = await response.Body.ReadAsStringAsync();
                    Console.WriteLine($"SendGrid Error Body: {errorBody}");
                    throw new Exception($"Failed to send email via SendGrid: {response.StatusCode} - {errorBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while sending email via SendGrid: {ex.Message}");
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


