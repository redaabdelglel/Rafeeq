



//"ApiKey": "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"


using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace Rafeeq.Services.Auth
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly SendGridClient _sendGridClient; // Reuse client instance

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            this._config = config;
            this._logger = logger;

            // Initialize SendGrid client once
            var sendGridApiKey = _config["SendGrid:ApiKey"];
            if (!string.IsNullOrEmpty(sendGridApiKey))
            {
                _sendGridClient = new SendGridClient(sendGridApiKey);
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (_sendGridClient == null)
            {
                throw new InvalidOperationException("SendGrid API Key is not configured.");
            }

            var fromEmailAddress = _config["EmailSettings:FromEmailAddress"] ?? throw new InvalidOperationException("FromEmailAddress is not configured.");
            var fromName = _config["EmailSettings:FromName"] ?? "Rafeeq Platform";

            var from = new EmailAddress(fromEmailAddress, fromName);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, message);

            // Optimize for faster delivery
            msg.AddHeader("X-Priority", "1");
            msg.AddHeader("Importance", "high");
            msg.AddHeader("X-MSMail-Priority", "High");

            // Add categories for better tracking and reputation
            msg.AddCategory("verification");
            msg.AddCategory("auth");
            msg.AddCategory("rafeeq");

            // Add custom args for tracking
            msg.AddCustomArg("email_type", "verification");
            msg.AddCustomArg("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            try
            {
                _logger.LogInformation("Sending verification email to {Email}", toEmail);

                var response = await _sendGridClient.SendEmailAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("SendGrid email failed for {Email}. Status: {StatusCode}. Body: {Body}",
                        toEmail, response.StatusCode, errorBody);
                    throw new Exception($"SendGrid failed to send email. Status: {response.StatusCode}");
                }

                _logger.LogInformation("Email sent successfully to {Email}. Status: {StatusCode}",
                    toEmail, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var verificationLink = $"{frontendUrl}/verify-email/{token}";

            var subject = "🚀 Verify Your Rafeeq Account - Quick Action Required";
            var message = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Verify Your Email</title>
                </head>
                <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #27ae60; margin: 0;'>Rafeeq</h1>
                            <p style='color: #666; margin: 5px 0 0 0;'>Professional Mentorship Platform</p>
                        </div>
                        
                        <div style='text-align: center;'>
                            <h2 style='color: #2c3e50; margin-bottom: 20px;'>Verify Your Email Address</h2>
                            <p style='color: #555; font-size: 16px; line-height: 1.6; margin-bottom: 30px;'>
                                Welcome to Rafeeq! Click the button below to verify your email and start your mentorship journey.
                            </p>
                            
                            <a href='{verificationLink}' 
                               style='display: inline-block; background-color: #27ae60; color: white; padding: 15px 30px; 
                                      text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;
                                      margin-bottom: 20px;'>
                                ✅ Verify Email Address
                            </a>
                            
                            <p style='color: #777; font-size: 14px; margin-top: 30px;'>
                                Or copy and paste this link in your browser:
                            </p>
                            <p style='word-break: break-all; color: #27ae60; font-size: 12px; 
                                      background-color: #f8f9fa; padding: 10px; border-radius: 4px;'>
                                {verificationLink}
                            </p>
                        </div>
                        
                        <div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #eee; text-align: center;'>
                            <p style='color: #999; font-size: 12px; margin: 0;'>
                                This link expires in 24 hours. If you didn't create this account, please ignore this email.
                            </p>
                            <p style='color: #999; font-size: 12px; margin: 10px 0 0 0;'>
                                © 2024 Rafeeq Platform. All rights reserved.
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, message);
        }

        // Update other email methods similarly...
        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?token={token}";

            var subject = "🔒 Rafeeq Password Reset Request";
            var message = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px;'>
                        <div style='text-align: center;'>
                            <h2 style='color: #3498db;'>Password Reset Request</h2>
                            <p>Click the button below to reset your password:</p>
                            <a href='{resetLink}' 
                               style='display: inline-block; background-color: #3498db; color: white; padding: 15px 30px; 
                                      text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                Reset Password
                            </a>
                            <p style='margin-top: 30px; color: #777; font-size: 12px;'>
                                This link expires in 1 hour.
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, message);
        }

        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType)
        {
            var subject = "💰 Rafeeq Payment Confirmation";

            string message;
            if (userType.ToLower() == "mentor")
            {
                message = $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px;'>
                            <h2 style='color: #27ae60;'>💰 Payment Received</h2>
                            <p>Dear {userName},</p>
                            <p>Good news! You've received a payment of <strong>${amount:F2}</strong> for booking #{bookingId}.</p>
                            <p>Session: <strong>{sessionDateTime:f}</strong></p>
                        </div>
                    </body>
                    </html>";
            }
            else
            {
                message = $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px;'>
                            <h2 style='color: #3498db;'>✅ Payment Confirmed</h2>
                            <p>Dear {userName},</p>
                            <p>Your payment of <strong>${amount:F2}</strong> for booking #{bookingId} was successful.</p>
                            <p>Session: <strong>{sessionDateTime:f}</strong></p>
                        </div>
                    </body>
                    </html>";
            }

            await SendEmailAsync(toEmail, subject, message);
        }
    }
}




//using SendGrid;
//using SendGrid.Helpers.Mail;

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
//            var sendGridApiKey = _config["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid API Key is not configured.");

//            var fromEmailAddress = _config["EmailSettings:FromEmailAddress"] ?? throw new InvalidOperationException("FromEmailAddress is not configured.");
//            var fromName = _config["EmailSettings:FromName"] ?? "Rafeeq Platform";

//            var client = new SendGridClient(sendGridApiKey);

//            var from = new EmailAddress(fromEmailAddress, fromName);

//            var to = new EmailAddress(toEmail);


//            var plainTextContent = string.Empty;
//            var htmlContent = message;


//            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

//            try
//            {
//                var response = await client.SendEmailAsync(msg);

//                if (!response.IsSuccessStatusCode)
//                {
//                    var errorBody = await response.Body.ReadAsStringAsync();
//                    Console.WriteLine($"SendGrid email failed for {toEmail}. Status Code: {response.StatusCode}. Body: {errorBody}");
//                    throw new Exception($"SendGrid failed to send email. Status: {response.StatusCode}, Body: {errorBody}");
//                }
//                Console.WriteLine($"Email sent successfully to {toEmail} via SendGrid. Status Code: {response.StatusCode}");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"An error occurred while sending email to {toEmail} via SendGrid: {ex.Message}");
//                throw;
//            }
//        }

//        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
//        {
//            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
//            var resetLink = $"{frontendUrl}/reset-password?token={token}";

//            var subject = "Rafeeq: Password Reset Request";
//            var message = $"You have requested a password reset. Click the link in your Email: <a href=\"{resetLink}\">{resetLink}</a>";

//            await SendEmailAsync(toEmail, subject, message);
//        }

//        public async Task SendVerificationEmailAsync(string toEmail, string token)
//        {
//            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
//            var verificationLink = $"{frontendUrl}/verify-email/{token}";
//            var subject = "Rafeeq: Verify Your Email Address";
//            var message = $"Verify your email by clicking on this link: <a href=\"{verificationLink}\">{verificationLink}</a>";

//            await SendEmailAsync(toEmail, subject, message);
//        }


//        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType)
//        {
//            var subject = "Rafeeq: Payment Confirmation";

//            string message;
//            if (userType.ToLower() == "mentor")
//            {
//                message = $@"
//                    <h2>Payment Received</h2>
//                    <p>Dear {userName},</p>
//                    <p>Good news! You've received a payment of ${amount:F2} for booking #{bookingId}.</p>
//                    <p>The session is scheduled for {sessionDateTime.ToString("f")}.</p>
//                    <p>Please log in to your dashboard to view more details.</p>
//                    <p>Thank you for being part of Rafeeq!</p>
//                    ";

//            }
//            else
//            {
//                message = $@"
//                    <h2>Payment Confirmation</h2>
//                    <p>Dear {userName},</p>
//                    <p>Your payment of ${amount:F2} for booking #{bookingId} was successful.</p>
//                    <p>The session is scheduled for {sessionDateTime.ToString("f")}.</p>
//                    <p>Please log in to your dashboard to view more details and join the session.</p>
//                    <p>Thank you for using Rafeeq!</p>
//                    ";
//            }

//            await SendEmailAsync(toEmail, subject, message);
//        }
//    }
//}