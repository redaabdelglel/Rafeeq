

using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace Rafeeq.Services.Auth
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly SendGridClient _sendGridClient;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            this._config = config;
            this._logger = logger;

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

        
            msg.AddHeader("X-Priority", "1");
            msg.AddHeader("Importance", "high");
            msg.AddHeader("X-MSMail-Priority", "High");
            msg.AddHeader("Precedence", "urgent");
            msg.AddHeader("Priority", "urgent");
            msg.AddHeader("X-Mailer", "Rafeeq-FastMail");

           
            msg.SetClickTracking(false, false);
            msg.SetOpenTracking(false);
            msg.SetSubscriptionTracking(false);
            msg.SetGoogleAnalytics(false);

            msg.SetBypassListManagement(false);

     
            msg.AddCategory("instant");
            msg.AddCategory("auth");
            msg.AddCategory("verified-sender");

     
            msg.AddCustomArg("speed", "max");
            msg.AddCustomArg("type", "auth");

            try
            {
                _logger.LogInformation("⚡ FAST: Sending instant email to {Email}", toEmail);

                var response = await _sendGridClient.SendEmailAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("❌ FAST EMAIL FAILED for {Email}. Status: {StatusCode}. Error: {Error}",
                        toEmail, response.StatusCode, errorBody);
                    throw new Exception($"Fast email delivery failed: {response.StatusCode} - {errorBody}");
                }

                _logger.LogInformation("✅ FAST EMAIL SUCCESS to {Email}. Status: {StatusCode}",
                    toEmail, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Fast email failed to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var verificationLink = $"{frontendUrl}/verify-email/{token}";

        
            _logger.LogInformation("Generated verification link: {Link} for email: {Email}", verificationLink, toEmail);

            var subject = "✅ Verify Your Rafeeq Account";
            var message = CreateFastVerificationTemplate(verificationLink);

            await SendEmailAsync(toEmail, subject, message);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?token={token}";

            
            _logger.LogInformation("Generated reset link: {Link} for email: {Email}", resetLink, toEmail);

            var subject = "🔐 Reset Your Rafeeq Password";
            var message = CreateFastPasswordResetTemplate(resetLink);

            await SendEmailAsync(toEmail, subject, message);
        }

       
        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType)
        {
            var subject = "💰 Rafeeq Payment Confirmation";
            var message = CreateFastPaymentTemplate(userName, bookingId, amount, sessionDateTime, userType);

           
            _logger.LogInformation("Sending payment confirmation email to {Email} for booking #{BookingId} - Amount: ${Amount}",
                toEmail, bookingId, amount);

            await SendEmailAsync(toEmail, subject, message);
        }

        
        private string CreateFastVerificationTemplate(string verificationLink)
        {
            return $@"<!DOCTYPE html>
<html><head><meta charset='UTF-8'><title>Verify Email</title></head>
<body style='font-family:Arial;margin:0;padding:20px;background:#f5f5f5'>
<div style='max-width:600px;margin:0 auto;background:#fff;padding:30px;border-radius:8px;box-shadow:0 2px 10px rgba(0,0,0,0.1)'>
<h1 style='color:#27ae60;text-align:center;margin:0 0 20px;font-size:28px'>Rafeeq</h1>
<h2 style='text-align:center;margin:0 0 20px;color:#333'>✅ Verify Your Email</h2>
<p style='text-align:center;margin:0 0 30px;color:#666;line-height:1.6'>Welcome to Rafeeq! Click the button below to verify your email and complete your registration:</p>

<!-- Main Verification Button -->
<div style='text-align:center;margin:30px 0'>
<a href='{verificationLink}' style='background:#27ae60;color:#fff;padding:16px 32px;text-decoration:none;border-radius:8px;font-weight:bold;display:inline-block;font-size:16px;border:2px solid #27ae60'>VERIFY NOW</a>
</div>

<!-- Alternative Text Link -->
<div style='text-align:center;margin:20px 0'>
<p style='color:#666;margin:0 0 10px;font-size:14px'>Or click this link:</p>
<a href='{verificationLink}' style='color:#27ae60;font-weight:bold;text-decoration:underline'>{verificationLink}</a>
</div>

<!-- Copy Link Section -->
<div style='background:#f8f9fa;padding:15px;border-radius:5px;margin:20px 0'>
<p style='color:#666;margin:0 0 10px;font-size:13px;font-weight:bold'>Can't click? Copy and paste this link:</p>
<p style='background:#fff;padding:10px;border:1px solid #ddd;border-radius:4px;margin:0;word-break:break-all;font-size:12px;color:#27ae60'>{verificationLink}</p>
</div>

<p style='text-align:center;color:#999;font-size:12px;margin:30px 0 0'>This link expires in 24 hours. If you didn't create this account, please ignore this email.</p>
</div></body></html>";
        }

        private string CreateFastPasswordResetTemplate(string resetLink)
        {
            return $@"<!DOCTYPE html>
<html><head><meta charset='UTF-8'><title>Reset Password</title></head>
<body style='font-family:Arial;margin:0;padding:20px;background:#f5f5f5'>
<div style='max-width:600px;margin:0 auto;background:#fff;padding:30px;border-radius:8px;box-shadow:0 2px 10px rgba(0,0,0,0.1)'>
<h1 style='color:#3498db;text-align:center;margin:0 0 20px;font-size:28px'>🔐 Reset Password</h1>
<p style='text-align:center;margin:0 0 30px;color:#666;line-height:1.6'>Click the button below to reset your password. This link expires in 1 hour:</p>

<!-- Main Reset Button -->
<div style='text-align:center;margin:30px 0'>
<a href='{resetLink}' style='background:#3498db;color:#fff;padding:16px 32px;text-decoration:none;border-radius:8px;font-weight:bold;display:inline-block;font-size:16px;border:2px solid #3498db'>RESET NOW</a>
</div>

<!-- Alternative Text Link -->
<div style='text-align:center;margin:20px 0'>
<p style='color:#666;margin:0 0 10px;font-size:14px'>Or click this link:</p>
<a href='{resetLink}' style='color:#3498db;font-weight:bold;text-decoration:underline'>{resetLink}</a>
</div>

<!-- Copy Link Section -->
<div style='background:#f8f9fa;padding:15px;border-radius:5px;margin:20px 0'>
<p style='color:#666;margin:0 0 10px;font-size:13px;font-weight:bold'>Can't click? Copy and paste this link:</p>
<p style='background:#fff;padding:10px;border:1px solid #ddd;border-radius:4px;margin:0;word-break:break-all;font-size:12px;color:#3498db'>{resetLink}</p>
</div>

<p style='text-align:center;color:#999;font-size:12px;margin:30px 0 0'>If you didn't request this reset, please ignore this email.</p>
</div></body></html>";
        }

       
        private string CreateFastPaymentTemplate(string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType)
        {
            var isForMentor = userType.ToLower() == "mentor";
            var emoji = isForMentor ? "💰" : "✅";
            var title = isForMentor ? "Payment Received" : "Payment Confirmed";
            var color = isForMentor ? "#27ae60" : "#3498db";
            var content = isForMentor
                ? $"Great news! You've received a payment of <strong>${amount:F2}</strong> for booking #{bookingId}."
                : $"Your payment of <strong>${amount:F2}</strong> for booking #{bookingId} has been processed successfully.";

            return $@"<!DOCTYPE html>
<html><head><meta charset='UTF-8'><title>Payment Confirmation</title></head>
<body style='font-family:Arial;margin:0;padding:20px;background:#f5f5f5'>
<div style='max-width:600px;margin:0 auto;background:#fff;padding:30px;border-radius:8px;box-shadow:0 2px 10px rgba(0,0,0,0.1)'>
<h1 style='color:{color};text-align:center;margin:0 0 20px;font-size:28px'>{emoji} {title}</h1>

<div style='text-align:center;margin:20px 0'>
<h2 style='color:#333;margin:0 0 20px'>Dear {userName},</h2>
<p style='margin:0 0 20px;color:#666;line-height:1.6'>{content}</p>
</div>

<!-- Payment Details Card -->
<div style='background:#f8f9fa;padding:20px;border-radius:8px;margin:20px 0;border-left:4px solid {color}'>
<h3 style='color:#333;margin:0 0 15px;font-size:18px'>Payment Details</h3>
<p style='margin:0 0 10px;color:#666'><strong>Booking ID:</strong> #{bookingId}</p>
<p style='margin:0 0 10px;color:#666'><strong>Amount:</strong> <span style='color:{color};font-weight:bold;font-size:18px'>${amount:F2}</span></p>
<p style='margin:0 0 10px;color:#666'><strong>Session Date:</strong> {sessionDateTime:f}</p>
<p style='margin:0;color:#666'><strong>Status:</strong> <span style='color:#27ae60;font-weight:bold'>Confirmed</span></p>
</div>

<!-- Action Section -->
<div style='text-align:center;margin:30px 0'>
<p style='margin:0 0 20px;color:#666'>Ready for your session? Log in to your dashboard for more details.</p>
<a href='http://localhost:4200/dashboard' style='background:{color};color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:bold;display:inline-block'>Go to Dashboard</a>
</div>

<div style='text-align:center;margin:30px 0 0;padding-top:20px;border-top:1px solid #eee'>
<p style='margin:0;color:{color};font-weight:bold;font-size:16px'>Thank you for using Rafeeq! 🚀</p>
<p style='margin:5px 0 0;color:#999;font-size:12px'>Professional Mentorship Platform</p>
</div>
</div></body></html>";
        }
    }
}








