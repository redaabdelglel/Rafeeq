namespace Rafeeq.Services.Auth
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendVerificationEmailAsync(string toEmail, string token);
        Task SendPasswordResetEmailAsync(string toEmail, string token);
        Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, int bookingId, decimal amount, DateTime sessionDateTime, string userType);
        //Task SendPasswordResetEmailAsync(string toEmail, string token);

    }

}
