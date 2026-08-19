using System.ComponentModel.DataAnnotations;

namespace EventPulse.Models
{
    /// <summary>
    /// Logs every notification the system sends (or would send). Kept separate from
    /// actually dispatching email so the record exists even if the SMTP call fails.
    /// </summary>
    public class Notification
    {
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool Sent { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}
