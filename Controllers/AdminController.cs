using EventPulse.Data;
using EventPulse.Models;
using EventPulse.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET /Admin — platform-wide overview
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalEvents = await _db.Events.CountAsync(),
                TotalOrganizers = (await _userManager.GetUsersInRoleAsync("Organizer")).Count,
                TotalAttendees = (await _userManager.GetUsersInRoleAsync("Attendee")).Count,
                RecentEvents = await _db.Events
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(10)
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
                    .ToListAsync()
            };

            return View(model);
        }

        // GET /Admin/Organizers — list + promote a user to Organizer
        public async Task<IActionResult> Organizers()
        {
            var organizers = await _userManager.GetUsersInRoleAsync("Organizer");
            return View(organizers);
        }

        // POST /Admin/PromoteToOrganizer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToOrganizer(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            user.Role = UserRole.Organizer;
            await _userManager.UpdateAsync(user);
            await _userManager.AddToRoleAsync(user, "Organizer");

            TempData["Success"] = $"{user.FullName} is now an Organizer.";
            return RedirectToAction(nameof(Organizers));
        }
    }
}
