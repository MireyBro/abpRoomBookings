using AbpRoomBookings.Domain.Entities;
using AbpRoomBookings.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AbpRoomBookings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HallsController : ControllerBase
{
    private readonly AppDbContext _db;

    public HallsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Hall hall)
    {
        hall.Id = Guid.NewGuid();
        _db.Halls.Add(hall);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = hall.Id }, new { id = hall.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Hall updated)
    {
        var hall = await _db.Halls.Include(h => h.HallServices).FirstOrDefaultAsync(h => h.Id == id);
        if (hall == null) return NotFound();
        hall.Name = updated.Name;
        hall.Capacity = updated.Capacity;
        hall.BaseHourlyPrice = updated.BaseHourlyPrice;
        // replace services if provided
        if (updated.HallServices?.Any() == true)
        {
            hall.HallServices.Clear();
            foreach (var hs in updated.HallServices)
            {
                hall.HallServices.Add(new HallService { HallId = hall.Id, ServiceId = hs.ServiceId });
            }
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var hall = await _db.Halls.FindAsync(id);
        if (hall == null) return NotFound();
        _db.Halls.Remove(hall);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("available")]
    public async Task<IActionResult> Available([FromQuery] DateTime date, [FromQuery] TimeSpan start, [FromQuery] TimeSpan end, [FromQuery] int capacity)
    {
        // Build start and end in Kyiv local
        var tz = TimeZoneHelper.GetKyivTimeZone();
        var localStart = DateTime.SpecifyKind(date.Date + start, DateTimeKind.Unspecified);
        var localEnd = DateTime.SpecifyKind(date.Date + end, DateTimeKind.Unspecified);
        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

        var halls = await _db.Halls
            .Include(h => h.HallServices).ThenInclude(hs => hs.Service)
            .Where(h => h.Capacity >= capacity)
            .ToListAsync();

        var available = new List<object>();
        foreach (var hall in halls)
        {
            var hasConflict = await _db.Bookings.AnyAsync(b => b.HallId == hall.Id &&
                b.StartUtc < utcEnd && b.EndUtc > utcStart);
            if (!hasConflict)
            {
                available.Add(new
                {
                    hall.Id,
                    hall.Name,
                    hall.Capacity,
                    hall.BaseHourlyPrice,
                    Services = hall.HallServices.Select(hs => new { hs.Service.Id, hs.Service.Name, hs.Service.Price })
                });
            }
        }
        return Ok(available);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var hall = await _db.Halls.Include(h => h.HallServices).ThenInclude(hs => hs.Service).FirstOrDefaultAsync(h => h.Id == id);
        if (hall == null) return NotFound();
        return Ok(hall);
    }
}
