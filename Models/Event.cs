using System.ComponentModel.DataAnnotations;

namespace EventPulse.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Required]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int Capacity { get; set; }

        [Range(0, int.MaxValue)]
        public int SeatsRemaining { get; set; }

        [Required]
        public string OrganizerId { get; set; } = string.Empty;
        public ApplicationUser? Organizer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<WaitlistEntry> WaitlistEntries { get; set; } = new List<WaitlistEntry>();
    }
}
