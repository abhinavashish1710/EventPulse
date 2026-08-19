using EventPulse.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EventPulse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.Controllers
{
    /// <summary>
    /// Read-only view of notifications sent to the current user — registration
    /// confirmations, cancellations, waitlist promotions. Sending itself happens
    /// inside INotificationService, called from Registrations/Payments controllers.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        /// <summary>Lists the current user's notifications, most recent first.</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> MyNotifications()
        {
            var userId = _userManager.GetUserId(User)!;

            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new { n.NotificationId, Type = n.Type.ToString(), n.Subject, n.Body, n.Sent, n.CreatedAt })
                .ToListAsync();

            return Ok(notifications);
        }
    }
}
