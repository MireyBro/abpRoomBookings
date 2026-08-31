using AbpRoomBookings.Domain.Entities;
using AbpRoomBookings.Domain.ValueObjects;

namespace AbpRoomBookings.Application.Services;

public interface IPricingService
{
    decimal CalculateTotal(decimal baseHourlyPrice, DateTime utcStart, double durationHours, List<decimal> servicePricesPerHour);
}

public class PricingService : IPricingService
{
    // Multipliers per rule
    // 06-09 -> -10% => 0.9
    // 09-18 -> 1.0
    // 12-14 -> +15% => 1.15 (peak overrides standard)
    // 18-23 -> -20% => 0.8

    public decimal CalculateTotal(decimal baseHourlyPrice, DateTime utcStart, double durationHours, List<decimal> servicePricesPerHour)
    {
        var total = 0m;
        var tz = TimeZoneHelper.GetKyivTimeZone();
        for (int i = 0; i < (int)Math.Ceiling(durationHours); i++)
        {
            var hourUtc = utcStart.AddHours(i);
            var hourLocal = TimeZoneInfo.ConvertTimeFromUtc(hourUtc, tz);
            var multiplier = GetMultiplier(hourLocal.Hour);
            total += baseHourlyPrice * (decimal)multiplier;
        }
        // services are charged per hour according to requirement
        var servicesPerHour = servicePricesPerHour.Sum();
        total += servicesPerHour * (decimal)durationHours;
        return total;
    }

    private double GetMultiplier(int hour)
    {
        // hour is 0-23
        if (hour >= 12 && hour < 14) return 1.15; // peak
        if (hour >= 18 && hour < 23) return 0.8; // evening
        if (hour >= 6 && hour < 9) return 0.9; // morning
        if (hour >= 9 && hour < 18) return 1.0; // standard
        return 1.0; // default
    }
}
