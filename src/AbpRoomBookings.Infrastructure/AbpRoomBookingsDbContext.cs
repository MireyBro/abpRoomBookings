using AbpRoomBookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbpRoomBookings.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<HallService> HallServices => Set<HallService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HallService>().HasKey(hs => new { hs.HallId, hs.ServiceId });
        modelBuilder.Entity<HallService>().HasOne(hs => hs.Hall).WithMany(h => h.HallServices).HasForeignKey(hs => hs.HallId);
        modelBuilder.Entity<HallService>().HasOne(hs => hs.Service).WithMany(s => s.HallServices).HasForeignKey(hs => hs.ServiceId);

        modelBuilder.Entity<BookingService>().HasKey(bs => new { bs.BookingId, bs.ServiceId });
        modelBuilder.Entity<BookingService>().HasOne(bs => bs.Booking).WithMany(b => b.BookingServices).HasForeignKey(bs => bs.BookingId);
        modelBuilder.Entity<BookingService>().HasOne(bs => bs.Service).WithMany().HasForeignKey(bs => bs.ServiceId);

        base.OnModelCreating(modelBuilder);
    }
}

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Services.Any())
        {
            var projector = new Service { Id = Guid.NewGuid(), Name = "Projector", Price = 500m };
            var wifi = new Service { Id = Guid.NewGuid(), Name = "Wi-Fi", Price = 300m };
            var sound = new Service { Id = Guid.NewGuid(), Name = "Sound", Price = 700m };
            db.Services.AddRange(projector, wifi, sound);

            var hallA = new Hall { Id = Guid.NewGuid(), Name = "Hall A", Capacity = 50, BaseHourlyPrice = 2000m };
            var hallB = new Hall { Id = Guid.NewGuid(), Name = "Hall B", Capacity = 100, BaseHourlyPrice = 3500m };
            var hallC = new Hall { Id = Guid.NewGuid(), Name = "Hall C", Capacity = 30, BaseHourlyPrice = 1500m };
            db.Halls.AddRange(hallA, hallB, hallC);

            db.HallServices.AddRange(
                new HallService { HallId = hallA.Id, ServiceId = projector.Id },
                new HallService { HallId = hallA.Id, ServiceId = wifi.Id },
                new HallService { HallId = hallB.Id, ServiceId = projector.Id },
                new HallService { HallId = hallB.Id, ServiceId = wifi.Id },
                new HallService { HallId = hallB.Id, ServiceId = sound.Id },
                new HallService { HallId = hallC.Id, ServiceId = wifi.Id }
            );

            db.Users.Add(new User { Id = Guid.NewGuid(), Username = "admin", Password = "password", Role = "Admin" });

            db.SaveChanges();
        }
    }
}
