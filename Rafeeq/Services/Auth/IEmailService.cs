namespace Rafeeq.Services.Auth
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendVerificationEmailAsync(string toEmail, string token);
        Task SendPasswordResetEmailAsync(string toEmail, string token);
    }
}
