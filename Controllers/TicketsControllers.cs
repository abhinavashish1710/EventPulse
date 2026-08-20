using EventPulse.Data;
using EventPulse.Models;
using EventPulse.Models.ViewModels;
using EventPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.Controllers
{
    /// <summary>
    /// Shows an attendee's QR ticket and serves the PNG encoded with Registration.QrCode.
    /// </summary>
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQrCodeService _qr;

        public TicketsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IQrCodeService qr)
        {
            _db = db;
            _userManager = userManager;
            _qr = qr;
        }

        // GET /Tickets/5
        public async Task<IActionResult> Show(int id)
        {
            var registration = await LoadIfAllowedAsync(id);
            if (registration is null) return NotFound();

            var model = new TicketViewModel
            {
                RegistrationId = registration.RegistrationId,
                EventName = registration.Event!.Name,
                Location = registration.Event.Location,
                EventDate = registration.Event.EventDate,
                AttendeeName = registration.User?.FullName ?? "Attendee",
                Status = registration.Status.ToString(),
                QrCode = registration.QrCode,
                CheckedIn = registration.CheckedIn,
                CanShowQr = registration.Status == RegistrationStatus.Confirmed && !string.IsNullOrEmpty(registration.QrCode)
            };

            return View(model);
        }

        // GET /Tickets/QrImage/5
        [HttpGet]
        public async Task<IActionResult> QrImage(int id)
        {
            var registration = await LoadIfAllowedAsync(id);
            if (registration is null) return NotFound();
            if (registration.Status != RegistrationStatus.Confirmed || string.IsNullOrEmpty(registration.QrCode))
                return NotFound();

            var png = _qr.GeneratePng(registration.QrCode);
            return File(png, "image/png");
        }

        private async Task<Registration?> LoadIfAllowedAsync(int id)
        {
            var registration = await _db.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration is null) return null;

            var userId = _userManager.GetUserId(User);
            var isOwner = registration.UserId == userId;
            var isEventOrganizer = registration.Event?.OrganizerId == userId;
            var isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isEventOrganizer && !isAdmin)
                return null;

            return registration;
        }
    }
}
