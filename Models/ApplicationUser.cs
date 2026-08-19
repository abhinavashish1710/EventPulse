using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EventPulse.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Attendee;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
