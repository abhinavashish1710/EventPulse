namespace EventPulse.Models
{
    public enum UserRole
    {
        Attendee,
        Organizer,
        Admin
    }

    public enum RegistrationStatus
    {
        Confirmed,
        Cancelled,
        Waitlisted
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Refunded,
        Failed
    }

    public enum NotificationType
    {
        RegistrationConfirmed,
        Cancelled,
        WaitlistPromoted,
        EventReminder
    }
}
