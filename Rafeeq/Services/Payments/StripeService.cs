using Microsoft.Extensions.Options;
using Rafeeq.Configurations;
using Rafeeq.Models;
using Stripe;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Rafeeq.Services.Payments
{
    public class StripeService
    {
        private readonly StripeSettings _stripeConfig;

        public StripeService(IOptions<StripeSettings> stripeConfig)
        {
            _stripeConfig = stripeConfig.Value;
            Stripe.StripeConfiguration.ApiKey = _stripeConfig.SecretKey;
        }

        // Create a payment intent
        public async Task<(string paymentIntentId, string clientSecret, long amount)> CreatePaymentIntentAsync(decimal amount, string currency = "usd", string description = null)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = ConvertToStripeAmount(amount, currency),
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Description = description,
                Metadata = new Dictionary<string, string>
                {
                    ["platform"] = "Rafeeq Mentorship Platform"
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return (paymentIntent.Id, paymentIntent.ClientSecret, paymentIntent.Amount);
        }

        // Confirm payment intent
        public async Task<bool> ConfirmPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                // Check payment status
                if (paymentIntent.Status == "succeeded")
                {
                    return true;
                }

                // For intents that need additional confirmation
                if (paymentIntent.Status == "requires_confirmation")
                {
                    var options = new PaymentIntentConfirmOptions();
                    paymentIntent = await service.ConfirmAsync(paymentIntentId, options);
                    return paymentIntent.Status == "succeeded";
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Refund payment
        public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Reason = RefundReasons.RequestedByCustomer
                };

                if (amount.HasValue)
                {
                    options.Amount = ConvertToStripeAmount(amount.Value);
                }

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                return refund.Status == "succeeded";
            }
            catch
            {
                return false;
            }
        }

        // Calculate platform commission
        public decimal CalculateCommission(decimal amount)
        {
            return amount * (_stripeConfig.PlatformCommissionPercentage / 100);
        }

        // Helper method to convert decimal to Stripe's long format (cents)
        private long ConvertToStripeAmount(decimal amount, string currency = "usd")
        {
            // Stripe uses smallest currency unit (cents for USD)
            return (long)(amount * 100);
        }
    }
}
