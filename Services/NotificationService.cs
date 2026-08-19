using EventPulse.Data;
using EventPulse.Models;

namespace EventPulse.Services
{
    public interface INotificationService
    {
        Task NotifyRegistrationConfirmedAsync(string userId, string eventName);
        Task NotifyCancellationAsync(string userId, string eventName);
        Task NotifyWaitlistPromotedAsync(string userId, string eventName);
    }

    /// <summary>
    /// Records every notification in the database and "sends" it. In this project,
    /// sending is simulated (logged as Sent = true) instead of calling a real SMTP
    /// server, in the same spirit as the mock payment gateway. Swapping in a real
    /// provider (SendGrid, Mailtrap, Gmail SMTP) means implementing this interface
    /// with an actual mail client call — nothing else in the app changes.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db) => _db = db;

        public Task NotifyRegistrationConfirmedAsync(string userId, string eventName) =>
            Log(userId, NotificationType.RegistrationConfirmed,
                "You're registered!",
                $"Your registration for \"{eventName}\" is confirmed. See you there!");

        public Task NotifyCancellationAsync(string userId, string eventName) =>
            Log(userId, NotificationType.Cancelled,
                "Registration cancelled",
                $"Your registration for \"{eventName}\" has been cancelled. Any payment made has been refunded.");

        public Task NotifyWaitlistPromotedAsync(string userId, string eventName) =>
            Log(userId, NotificationType.WaitlistPromoted,
                "A seat opened up!",
                $"A seat for \"{eventName}\" became available and you've been moved from the waitlist to a confirmed registration.");

        private async Task Log(string userId, NotificationType type, string subject, string body)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Subject = subject,
                Body = body,
                Sent = true,
                SentAt = DateTime.UtcNow
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }
    }
}
