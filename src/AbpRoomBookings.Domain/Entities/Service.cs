namespace AbpRoomBookings.Domain.Entities;

public class Service
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<HallService> HallServices { get; set; } = new List<HallService>();
}
