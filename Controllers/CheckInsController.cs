using EventPulse.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.Controllers
{
    /// <summary>
    /// Handles scanning an attendee's QR code at the event to mark them checked in.
    /// Each QR code is single-use — a repeat scan is rejected, not silently ignored.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Organizer,Admin")]
    public class CheckInsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CheckInsController(ApplicationDbContext db) => _db = db;

        /// <summary>Scans a QR code and marks that registration as checked in.</summary>
        [HttpPost]
        public async Task<IActionResult> Scan([FromQuery] string qrCode)
        {
            var registration = await _db.Registrations.FirstOrDefaultAsync(r => r.QrCode == qrCode);

            if (registration is null)
                return Ok(new { success = false, message = "QR code not recognized." });

            if (registration.Status != Models.RegistrationStatus.Confirmed)
                return Ok(new { success = false, message = "Registration is not confirmed (cancelled or waitlisted)." });

            if (registration.CheckedIn)
                return Ok(new { success = false, message = "This ticket has already been checked in." });

            registration.CheckedIn = true;
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Check-in successful." });
        }
    }
}
