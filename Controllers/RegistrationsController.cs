using EventPulse.Data;
using EventPulse.Models;
using EventPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.Controllers
{
    /// <summary>
    /// Handles registering for events and cancelling registrations.
    /// Registration is transaction-safe to prevent two attendees double-booking
    /// the same last seat; cancellation frees the seat, refunds if paid, and
    /// auto-promotes the next waitlisted attendee.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RegistrationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notifications;

        public RegistrationsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, INotificationService notifications)
        {
            _db = db;
            _userManager = userManager;
            _notifications = notifications;
        }

        /// <summary>Registers the current user for an event, if a seat is available.</summary>
        [HttpPost]
        public async Task<IActionResult> Register([FromQuery] int eventId)
        {
            var userId = _userManager.GetUserId(User)!;

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (ev is null) return NotFound("Event not found.");

            var alreadyRegistered = await _db.Registrations
                .AnyAsync(r => r.EventId == eventId && r.UserId == userId && r.Status != RegistrationStatus.Cancelled);
            if (alreadyRegistered)
                return BadRequest("You are already registered for this event.");

            if (ev.SeatsRemaining <= 0)
                return Conflict("Event is full. Join the waitlist instead (POST /api/waitlist).");

            ev.SeatsRemaining -= 1;

            var registration = new Registration
            {
                EventId = eventId,
                UserId = userId,
                Status = RegistrationStatus.Confirmed,
                QrCode = Guid.NewGuid().ToString("N")
            };

            _db.Registrations.Add(registration);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _notifications.NotifyRegistrationConfirmedAsync(userId, ev.Name);

            return Ok(new
            {
                registration.RegistrationId,
                Status = registration.Status.ToString(),
                registration.QrCode,
                RequiresPayment = ev.Price > 0,
                AmountDue = ev.Price > 0 ? ev.Price : 0
            });
        }

        /// <summary>Lists the current user's registrations.</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> MyRegistrations()
        {
            var userId = _userManager.GetUserId(User)!;

            var registrations = await _db.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .Select(r => new
                {
                    r.RegistrationId,
                    EventName = r.Event!.Name,
                    r.Event.EventDate,
                    Status = r.Status.ToString(),
                    r.QrCode,
                    r.CheckedIn
                })
                .ToListAsync();

            return Ok(registrations);
        }

        /// <summary>Cancels a registration: frees the seat, refunds if paid, auto-promotes the waitlist.</summary>
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var registration = await _db.Registrations
                .Include(r => r.Payment)
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration is null) return NotFound();
            if (registration.UserId != userId) return Forbid();
            if (registration.Status == RegistrationStatus.Cancelled) return BadRequest("Already cancelled.");

            registration.Status = RegistrationStatus.Cancelled;

            if (registration.Payment is { Status: PaymentStatus.Completed } payment)
                payment.Status = PaymentStatus.Refunded;

            var ev = registration.Event!;
            ev.SeatsRemaining += 1;

            var nextInLine = await _db.WaitlistEntries
                .Where(w => w.EventId == ev.EventId)
                .OrderBy(w => w.Position)
                .FirstOrDefaultAsync();

            if (nextInLine is not null)
            {
                ev.SeatsRemaining -= 1;

                var promoted = new Registration
                {
                    EventId = ev.EventId,
                    UserId = nextInLine.UserId,
                    Status = RegistrationStatus.Confirmed,
                    QrCode = Guid.NewGuid().ToString("N")
                };
                _db.Registrations.Add(promoted);
                _db.WaitlistEntries.Remove(nextInLine);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _notifications.NotifyCancellationAsync(userId, ev.Name);
            if (nextInLine is not null)
                await _notifications.NotifyWaitlistPromotedAsync(nextInLine.UserId, ev.Name);

            return NoContent();
        }
    }
}
