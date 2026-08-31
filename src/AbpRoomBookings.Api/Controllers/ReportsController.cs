using AbpRoomBookings.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbpRoomBookings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db) { _db = db; }

    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var fromUtc = from.ToUniversalTime();
        var toUtc = to.ToUniversalTime();
        var total = await _db.Bookings.Where(b => b.StartUtc >= fromUtc && b.StartUtc <= toUtc).SumAsync(b => (decimal?)b.TotalPrice) ?? 0;
        var byHall = await _db.Bookings.Where(b => b.StartUtc >= fromUtc && b.StartUtc <= toUtc)
            .GroupBy(b => b.HallId)
            .Select(g => new { HallId = g.Key, Revenue = g.Sum(b => b.TotalPrice) })
            .ToListAsync();
        return Ok(new { Total = total, ByHall = byHall });
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> Occupancy([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var totalPeriodHours = (to - from).TotalHours;
        var halls = await _db.Halls.ToListAsync();
        var res = new List<object>();
        foreach (var h in halls)
        {
            var bookedHours = await _db.Bookings.Where(b => b.HallId == h.Id && b.StartUtc < to.ToUniversalTime() && b.EndUtc > from.ToUniversalTime())
                .SumAsync(b => (double?)b.DurationHours) ?? 0;
            var occupancy = totalPeriodHours > 0 ? bookedHours / totalPeriodHours : 0;
            res.Add(new { HallId = h.Id, HallName = h.Name, Occupancy = occupancy });
        }
        return Ok(res);
    }

    [HttpGet("popular-services")]
    public async Task<IActionResult> PopularServices([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var fromUtc = from.ToUniversalTime();
        var toUtc = to.ToUniversalTime();
        var data = await _db.BookingServices
            .Where(bs => bs.Booking.StartUtc >= fromUtc && bs.Booking.StartUtc <= toUtc)
            .GroupBy(bs => bs.ServiceId)
            .Select(g => new { ServiceId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        return Ok(data);
    }
}
