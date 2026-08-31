namespace AbpRoomBookings.Domain.Entities;

public class HallService
{
    public Guid HallId { get; set; }
    public Hall Hall { get; set; } = default!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = default!;
}
