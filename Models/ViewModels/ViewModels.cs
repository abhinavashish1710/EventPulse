using System.ComponentModel.DataAnnotations;

namespace EventPulse.Models.ViewModels
{
    public class EventListItemViewModel
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
        public decimal Price { get; set; }
        public int SeatsRemaining { get; set; }
        public int Capacity { get; set; }
    }

    public class EventFormViewModel
    {
        public int EventId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Required, Display(Name = "Event Date")]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; }

        public string? Location { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Range(1, 100000)]
        public int Capacity { get; set; }
    }

    public class AttendeeDashboardViewModel
    {
        public List<EventListItemViewModel> UpcomingEvents { get; set; } = new();
        public List<RegistrationSummaryViewModel> MyRegistrations { get; set; } = new();
    }

    public class RegistrationSummaryViewModel
    {
        public int RegistrationId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? QrCode { get; set; }
        public bool CheckedIn { get; set; }
    }

    public class OrganizerDashboardViewModel
    {
        public List<EventListItemViewModel> MyEvents { get; set; } = new();
        public int TotalRegistrations { get; set; }
        public int TotalCheckedIn { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalOrganizers { get; set; }
        public int TotalAttendees { get; set; }
        public List<EventListItemViewModel> RecentEvents { get; set; } = new();
    }
}
