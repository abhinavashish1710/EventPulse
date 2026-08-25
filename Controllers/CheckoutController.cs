using EventPulse.Data;
using EventPulse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.Controllers
{
    /// <summary>
    /// Renders the mock payment page a user is sent to after registering for a paid
    /// event. The actual payment logic lives in PaymentsController — this controller
    /// only shows the checkout screen and reuses that API endpoint via JS.
    /// </summary>
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET /Checkout/Pay?registrationId=5
        public async Task<IActionResult> Pay(int registrationId)
        {
            var userId = _userManager.GetUserId(User)!;

            var registration = await _db.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

            if (registration is null) return NotFound();
            if (registration.UserId != userId) return Forbid();

            if (registration.Status != RegistrationStatus.AwaitingPayment)
            {
                // Already paid (or cancelled) — nothing to check out, send them to their dashboard.
                return RedirectToAction("Attendee", "Dashboard");
            }

            ViewBag.RegistrationId = registration.RegistrationId;
            ViewBag.EventName = registration.Event!.Name;
            ViewBag.Amount = registration.Event.Price;

            return View();
        }
    }
}
