


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

            // Headers for immediate delivery
            msg.AddHeader("X-Priority", "1");
            msg.AddHeader("Importance", "high");
            msg.AddHeader("X-MSMail-Priority", "High");
            msg.AddHeader("List-Unsubscribe", $"<mailto:unsubscribe@{GetDomainFromEmail(fromEmailAddress)}>");

            // Categories for better reputation
            msg.AddCategory("transactional");
            msg.AddCategory("authentication");
            msg.AddCategory("high-priority");

            // Disable tracking to avoid delays
            msg.SetClickTracking(false, false);
            msg.SetOpenTracking(false);
            msg.SetSubscriptionTracking(false);

            // Custom args for debugging
            msg.AddCustomArg("email_type", "auth");
            msg.AddCustomArg("sent_at", DateTime.UtcNow.ToString("o"));

            try
            {
                _logger.LogInformation("Sending email to {Email} with subject: {Subject}", toEmail, subject);

                var response = await _sendGridClient.SendEmailAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("SendGrid failed for {Email}. Status: {StatusCode}. Error: {Error}",
                        toEmail, response.StatusCode, errorBody);
                    throw new Exception($"Email delivery failed: {response.StatusCode}");
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

            var subject = "Verify Your Rafeeq Account";
            var message = CreateVerificationEmailTemplate(verificationLink);

            await SendEmailAsync(toEmail, subject, message);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?token={token}";

            var subject = "Reset Your Rafeeq Password";
            var message = CreatePasswordResetTemplate(resetLink);

            await SendEmailAsync(toEmail, subject, message);
        }

        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType)
        {
            var subject = "Rafeeq Payment Confirmation";
            var message = CreatePaymentTemplate(userName, bookingId, amount, sessionDateTime, userType);

            await SendEmailAsync(toEmail, subject, message);
        }

        private string CreateVerificationEmailTemplate(string verificationLink)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Verify Email</title>
</head>
<body style='margin: 0; padding: 20px; font-family: Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background-color: #27ae60; padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>Rafeeq</h1>
                            <p style='color: #ffffff; margin: 5px 0 0 0; opacity: 0.9;'>Professional Mentorship Platform</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 40px 30px; text-align: center;'>
                            <h2 style='color: #333333; margin: 0 0 20px 0;'>Verify Your Email</h2>
                            <p style='color: #666666; margin: 0 0 30px 0; line-height: 1.6;'>
                                Welcome to Rafeeq! Click the button below to verify your email address and complete your registration.
                            </p>
                            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                <tr>
                                    <td align='center'>
                                        <a href='{verificationLink}' style='display: inline-block; background-color: #27ae60; color: #ffffff; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>
                                            Verify Email Address
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style='color: #999999; font-size: 12px; margin: 30px 0 0 0;'>
                                This link expires in 24 hours. If you didn't create this account, please ignore this email.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string CreatePasswordResetTemplate(string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Password</title>
</head>
<body style='margin: 0; padding: 20px; font-family: Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden;'>
                    <tr>
                        <td style='background-color: #3498db; padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0;'>Rafeeq</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 40px 30px; text-align: center;'>
                            <h2 style='color: #333333; margin: 0 0 20px 0;'>Reset Your Password</h2>
                            <p style='color: #666666; margin: 0 0 30px 0;'>
                                Click the button below to reset your password. This link expires in 1 hour.
                            </p>
                            <a href='{resetLink}' style='display: inline-block; background-color: #3498db; color: #ffffff; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                Reset Password
                            </a>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string CreatePaymentTemplate(string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType)
        {
            var isForMentor = userType.ToLower() == "mentor";
            var title = isForMentor ? "Payment Received" : "Payment Confirmed";
            var color = isForMentor ? "#27ae60" : "#3498db";
            var content = isForMentor 
                ? $"You've received a payment of ${amount:F2} for booking #{bookingId}."
                : $"Your payment of ${amount:F2} for booking #{bookingId} was successful.";

            return $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px;'>
    <table width='600' cellpadding='0' cellspacing='0' border='0' style='background-color: #ffffff; margin: 0 auto; border-radius: 8px;'>
        <tr>
            <td style='padding: 30px; text-align: center;'>
                <h2 style='color: {color}; margin: 0 0 20px 0;'>{title}</h2>
                <p>Dear {userName},</p>
                <p>{content}</p>
                <p>Session: <strong>{sessionDateTime:f}</strong></p>
                <p>Thank you for using Rafeeq!</p>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string GetDomainFromEmail(string email)
        {
            return email.Substring(email.IndexOf('@') + 1);
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

