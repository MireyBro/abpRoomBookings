using AbpRoomBookings.Domain.Entities;
using AbpRoomBookings.Infrastructure;
using AbpRoomBookings.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbpRoomBookings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;

    public BookingsController(AppDbContext db, IPricingService pricing)
    {
        _db = db;
        _pricing = pricing;
    }

    public record CreateBookingRequest(Guid HallId, DateTime Date, TimeSpan Start, double DurationHours, List<Guid> ServiceIds);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest req)
    {
        var hall = await _db.Halls.Include(h => h.HallServices).ThenInclude(hs => hs.Service).FirstOrDefaultAsync(h => h.Id == req.HallId);
        if (hall == null) return BadRequest("Hall not found");

        var tz = TimeZoneHelper.GetKyivTimeZone();
        var localStart = DateTime.SpecifyKind(req.Date.Date + req.Start, DateTimeKind.Unspecified);
        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
        var utcEnd = utcStart.AddHours(req.DurationHours);

        var conflict = await _db.Bookings.AnyAsync(b => b.HallId == req.HallId && b.StartUtc < utcEnd && b.EndUtc > utcStart);
        if (conflict) return Conflict("Hall is already booked for this time");

        var services = await _db.Services.Where(s => req.ServiceIds.Contains(s.Id)).ToListAsync();

        var totalPrice = _pricing.CalculateTotal(hall.BaseHourlyPrice, utcStart, req.DurationHours, services.Select(s => s.Price).ToList());

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HallId = hall.Id,
            StartUtc = utcStart,
            DurationHours = req.DurationHours,
            EndUtc = utcEnd,
            TotalPrice = totalPrice
        };
        foreach (var s in services)
        {
            booking.BookingServices.Add(new BookingService { BookingId = booking.Id, ServiceId = s.Id });
        }

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = booking.Id }, new { booking.Id, booking.TotalPrice });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var booking = await _db.Bookings
            .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
            .Include(b => b.Hall)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (booking == null) return NotFound();
        return Ok(booking);
    }
}
