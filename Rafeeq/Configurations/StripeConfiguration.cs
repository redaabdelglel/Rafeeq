namespace Rafeeq.Configurations
{
    public class StripeConfiguration
    {
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }
        public string WebhookSecret { get; set; }
        public decimal PlatformCommissionPercentage { get; set; } = 20; // Default 20% commission
    }
}
