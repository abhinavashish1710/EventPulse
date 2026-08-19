# Migrations

No migration files are included yet — they need to be generated on your machine
(this build environment doesn't have the .NET SDK or EF Core CLI tools installed).

Run this from the project folder, or use the Package Manager Console in Visual Studio:

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Or in Visual Studio's Package Manager Console:

```
Add-Migration InitialCreate
Update-Database
```

This will create the `EventPulse` database on your local `(localdb)\MSSQLLocalDB`
instance (already configured in `appsettings.json`) with all tables: AspNetUsers,
AspNetRoles, Events, Registrations, Payments, WaitlistEntries, Notifications.

The app also calls `DbSeeder.SeedAsync()` on startup, which seeds the 3 roles
(Attendee, Organizer, Admin), three demo accounts, and one sample event —
so right after `Update-Database` + running the app, there's data to show.

Demo logins (all seeded automatically on first run):
- admin@eventpulse.com / Admin@123
- organizer@eventpulse.com / Organizer@123
- attendee@eventpulse.com / Attendee@123
