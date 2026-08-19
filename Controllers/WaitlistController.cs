using EventPulse.Data;
using EventPulse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.Controllers
{
    /// <summary>
    /// Lets attendees join a FIFO waitlist when an event is full. Promotion off
    /// the waitlist happens automatically from RegistrationsController.Cancel.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WaitlistController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public WaitlistController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        /// <summary>Joins the waitlist for a full event.</summary>
        [HttpPost]
        public async Task<IActionResult> Join([FromQuery] int eventId)
        {
            var userId = _userManager.GetUserId(User)!;

            var ev = await _db.Events.FindAsync(eventId);
            if (ev is null) return NotFound("Event not found.");
            if (ev.SeatsRemaining > 0) return BadRequest("Seats are available — register directly instead.");

            var alreadyOnList = await _db.WaitlistEntries.AnyAsync(w => w.EventId == eventId && w.UserId == userId);
            if (alreadyOnList) return BadRequest("Already on the waitlist for this event.");

            var currentMax = await _db.WaitlistEntries
                .Where(w => w.EventId == eventId)
                .Select(w => (int?)w.Position)
                .MaxAsync() ?? 0;

            var entry = new WaitlistEntry
            {
                EventId = eventId,
                UserId = userId,
                Position = currentMax + 1
            };
            _db.WaitlistEntries.Add(entry);
            await _db.SaveChangesAsync();

            return Ok(new { entry.WaitlistEntryId, entry.Position });
        }

        /// <summary>Organizer/Admin view of an event's waitlist, in order.</summary>
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            var entries = await _db.WaitlistEntries
                .Where(w => w.EventId == eventId)
                .OrderBy(w => w.Position)
                .Include(w => w.User)
                .Select(w => new { w.WaitlistEntryId, w.Position, UserName = w.User!.FullName, w.JoinedAt })
                .ToListAsync();

            return Ok(entries);
        }
    }
}
