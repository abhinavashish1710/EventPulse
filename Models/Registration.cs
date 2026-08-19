using System.ComponentModel.DataAnnotations;

namespace EventPulse.Models
{
    public class Registration
    {
        public int RegistrationId { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event? Event { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;

        [MaxLength(100)]
        public string? QrCode { get; set; }

        public bool CheckedIn { get; set; } = false;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public Payment? Payment { get; set; }
    }
}
