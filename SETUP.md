# EventPulse — Setup Guide

## What this is
A complete ASP.NET Core (.NET 10) project: MVC + Razor views (CSHTML) for the
front end, Web API controllers for Registrations/Payments/Waitlist/CheckIns/
Notifications (visible in Swagger), Entity Framework Core against SQL Server,
and ASP.NET Core Identity for auth and roles.

## 1. Open the project
Open `EventPulse.sln` in Visual Studio 2022+ (or VS "26" / any version that
supports .NET 10 SDK). Make sure the .NET 10 SDK is installed.

## 2. Connection string
Already set in `appsettings.json` to point at your local instance:
```
Server=(localdb)\MSSQLLocalDB;Database=EventPulse;Trusted_Connection=True;...
```
This matches the `(localdb)\MSSQLLocalDB` instance visible in your SQL Server
Object Explorer — no changes needed unless your instance name is different.

## 3. Create the database
In Visual Studio's **Package Manager Console** (Tools → NuGet Package Manager →
Package Manager Console):
```
Add-Migration InitialCreate
Update-Database
```
This creates the `EventPulse` database with all tables (Identity tables +
Events, Registrations, Payments, WaitlistEntries, Notifications).

## 4. Run the project
Press F5 (or `dotnet run`). On first run, `DbSeeder` automatically seeds:
- 3 roles: Attendee, Organizer, Admin
- 3 demo accounts (see below)
- 1 sample event

Demo logins:
| Role | Email | Password |
|---|---|---|
| Admin | admin@eventpulse.com | Admin@123 |
| Organizer | organizer@eventpulse.com | Organizer@123 |
| Attendee | attendee@eventpulse.com | Attendee@123 |

## 5. What to show

**CSHTML front end:**
- `/` — home page
- `/Events` — browse/search events
- `/Events/Create` — create an event (log in as Organizer first)
- `/Dashboard` — role-based dashboard (redirects based on logged-in role)
- `/Admin` — platform overview (log in as Admin)

**Swagger (API docs):**
- `/swagger` — interactive API documentation for the Web API controllers:
  Registrations, Payments, Waitlist, CheckIns, Notifications.
  You can execute real requests directly from this page — useful for
  demoing the registration/payment/refund flow without clicking through the UI.

**Database:**
- SQL Server Object Explorer → `(localdb)\MSSQLLocalDB` → Databases →
  `EventPulse` → Tables — shows all tables with real seeded data after step 3–4.

## Notes on what's mocked
- **Payments** — `MockPaymentGateway` simulates a gateway (delay + ~95% success
  rate) instead of calling Razorpay/Stripe. Swapping in a real one later means
  writing one new class implementing `IPaymentGateway`.
- **Notifications** — `NotificationService` logs notifications to the database
  and marks them "Sent" instead of calling a real SMTP server. Swapping in
  real email means implementing `INotificationService` with an actual mail
  client call (e.g. MailKit + Mailtrap/SendGrid).
