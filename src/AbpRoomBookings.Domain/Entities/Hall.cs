using System.ComponentModel.DataAnnotations.Schema;

namespace AbpRoomBookings.Domain.Entities;

public class Hall
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyPrice { get; set; }
    public List<HallService> HallServices { get; set; } = new List<HallService>();
}
