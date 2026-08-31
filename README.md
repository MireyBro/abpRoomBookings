# ABP Room Bookings

This project implements a simple conference room booking API using ASP.NET Core, EF Core (SQLite) and JWT authentication.

Features:
- Create / Update / Delete halls (Admin only)
- Search available halls for a given date/time and capacity
- Create bookings and calculate total price with time-based multipliers and per-hour services
- Reports: revenue, occupancy, popular services
- Swagger UI available at /swagger

Getting started:

1. Build and run

cd src/AbpRoomBookings.Api
dotnet restore
dotnet run

The API will create a local SQLite database `abprooms.db` and seed initial data (Halls A/B/C and services).

2. Authentication

POST /api/auth/login
{
  "username": "admin",
  "password": "password"
}

Response contains a JWT token. Use it as `Authorization: Bearer {token}` for admin endpoints.

3. Example: Search available halls
GET /api/halls/available?date=2024-09-01&start=10:00:00&end=14:00:00&capacity=50

4. Book a hall
POST /api/bookings
{
  "hallId": "{id}",
  "date": "2024-09-01",
  "start": "10:00:00",
  "durationHours": 4,
  "serviceIds": [ "{serviceId1}", "{serviceId2}" ]
}

Notes:
- Times are interpreted in Europe/Kyiv timezone and stored in UTC.
- Services are charged per hour as requested.
- Pricing rules are implemented per the task.

Security:
- For demo purposes, user passwords are stored in plaintext and JWT key is in appsettings. Do NOT use this in production.

