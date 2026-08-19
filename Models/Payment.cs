using System.ComponentModel.DataAnnotations;

namespace EventPulse.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required]
        public int RegistrationId { get; set; }
        public Registration? Registration { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(30)]
        public string? PaymentMethod { get; set; }

        [MaxLength(60)]
        public string? TransactionRef { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}
