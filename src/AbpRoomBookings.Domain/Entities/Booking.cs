namespace AbpRoomBookings.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid HallId { get; set; }
    public Hall Hall { get; set; } = default!;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public double DurationHours { get; set; }
    public decimal TotalPrice { get; set; }
    public List<BookingService> BookingServices { get; set; } = new List<BookingService>();
}

public class BookingService
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = default!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = default!;
}
