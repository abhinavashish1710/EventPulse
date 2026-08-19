using EventPulse.Data;
using EventPulse.Models;
using EventPulse.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET /Dashboard  — redirects based on the logged-in user's role
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            return user.Role switch
            {
                UserRole.Organizer => RedirectToAction(nameof(Organizer)),
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction(nameof(Attendee))
            };
        }

        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> Attendee()
        {
            var userId = _userManager.GetUserId(User)!;

            var model = new AttendeeDashboardViewModel
            {
                UpcomingEvents = await _db.Events
                    .Where(e => e.EventDate >= DateTime.UtcNow &&
                                e.Registrations.Any(r => r.UserId == userId && r.Status == RegistrationStatus.Confirmed))
                    .Select(e => new EventListItemViewModel
                    {
                        EventId = e.EventId,
                        Name = e.Name,
                        Category = e.Category,
                        EventDate = e.EventDate,
                        Location = e.Location,
                        Price = e.Price,
                        SeatsRemaining = e.SeatsRemaining,
                        Capacity = e.Capacity
                    })
                    .ToListAsync(),

                MyRegistrations = await _db.Registrations
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Event)
                    .Select(r => new RegistrationSummaryViewModel
                    {
                        RegistrationId = r.RegistrationId,
                        EventName = r.Event!.Name,
                        EventDate = r.Event.EventDate,
                        Status = r.Status.ToString(),
                        QrCode = r.QrCode,
                        CheckedIn = r.CheckedIn
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Organizer()
        {
            var organizerId = _userManager.GetUserId(User)!;

            var myEvents = await _db.Events.Where(e => e.OrganizerId == organizerId).ToListAsync();
            var eventIds = myEvents.Select(e => e.EventId).ToList();

            var model = new OrganizerDashboardViewModel
            {
                MyEvents = myEvents.Select(e => new EventListItemViewModel
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Category = e.Category,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    Price = e.Price,
                    SeatsRemaining = e.SeatsRemaining,
                    Capacity = e.Capacity
                }).ToList(),

                TotalRegistrations = await _db.Registrations
                    .CountAsync(r => eventIds.Contains(r.EventId) && r.Status == RegistrationStatus.Confirmed),

                TotalCheckedIn = await _db.Registrations
                    .CountAsync(r => eventIds.Contains(r.EventId) && r.CheckedIn),

                TotalRevenue = await _db.Payments
                    .Where(p => eventIds.Contains(p.Registration!.EventId) && p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0
            };

            return View(model);
        }
    }
}
