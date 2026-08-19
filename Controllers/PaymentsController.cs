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
    /// Handles the (mock) payment lifecycle for paid registrations: creating a
    /// payment, running it through the simulated gateway, and processing refunds.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPaymentGateway _gateway;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentsController(ApplicationDbContext db, IPaymentGateway gateway, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _gateway = gateway;
            _userManager = userManager;
        }

        /// <summary>Runs a registration's payment through the mock gateway.</summary>
        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromQuery] int registrationId, [FromQuery] string paymentMethod = "Card")
        {
            var registration = await _db.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

            if (registration is null) return NotFound("Registration not found.");

            var payment = new Payment
            {
                RegistrationId = registration.RegistrationId,
                Amount = registration.Event!.Price,
                Status = PaymentStatus.Pending,
                PaymentMethod = paymentMethod
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            var result = await _gateway.ProcessPaymentAsync(payment.Amount, paymentMethod);

            payment.Status = result.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.TransactionRef = result.TransactionRef;
            await _db.SaveChangesAsync();

            if (!result.Success)
                return BadRequest(new { payment.Status, reason = result.FailureReason });

            return Ok(new { payment.PaymentId, payment.RegistrationId, payment.Amount, Status = payment.Status.ToString(), payment.TransactionRef });
        }

        /// <summary>Refunds a completed payment (normally triggered from cancellation).</summary>
        [HttpPost("{paymentId:int}/refund")]
        public async Task<IActionResult> Refund(int paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment is null) return NotFound();
            if (payment.Status != PaymentStatus.Completed) return BadRequest("Only completed payments can be refunded.");

            var result = await _gateway.ProcessRefundAsync(payment.TransactionRef ?? paymentId.ToString(), payment.Amount);

            if (result.Success)
                payment.Status = PaymentStatus.Refunded;

            await _db.SaveChangesAsync();
            return Ok(new { payment.PaymentId, Status = payment.Status.ToString() });
        }

        /// <summary>Lists the current user's payment/refund history.</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> MyPayments()
        {
            var userId = _userManager.GetUserId(User)!;

            var payments = await _db.Payments
                .Where(p => p.Registration!.UserId == userId)
                .Select(p => new { p.PaymentId, p.RegistrationId, p.Amount, Status = p.Status.ToString(), p.TransactionDate })
                .ToListAsync();

            return Ok(payments);
        }
    }
}
