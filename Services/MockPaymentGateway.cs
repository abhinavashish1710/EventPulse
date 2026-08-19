namespace EventPulse.Services
{
    public record PaymentResult(bool Success, string TransactionRef, string? FailureReason = null);

    public interface IPaymentGateway
    {
        Task<PaymentResult> ProcessPaymentAsync(decimal amount, string paymentMethod);
        Task<PaymentResult> ProcessRefundAsync(string transactionRef, decimal amount);
    }

    /// <summary>
    /// Simulates a payment gateway — no external calls. Kept behind IPaymentGateway
    /// so a real provider (Razorpay/Stripe) could be swapped in later by adding one
    /// new class and changing a single line in Program.cs.
    /// </summary>
    public class MockPaymentGateway : IPaymentGateway
    {
        private readonly Random _random = new();

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string paymentMethod)
        {
            await Task.Delay(400);
            bool success = _random.NextDouble() > 0.05; // ~95% success rate

            if (!success)
                return new PaymentResult(false, string.Empty, "Card declined (simulated).");

            var transactionRef = $"MOCK-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
            return new PaymentResult(true, transactionRef);
        }

        public async Task<PaymentResult> ProcessRefundAsync(string transactionRef, decimal amount)
        {
            await Task.Delay(250);
            var refundRef = $"REFUND-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
            return new PaymentResult(true, refundRef);
        }
    }
}
