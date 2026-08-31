namespace AbpRoomBookings.Domain.ValueObjects;

public static class TimeZoneHelper
{
    public static TimeZoneInfo GetKyivTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        }
        catch
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev"); } catch { return TimeZoneInfo.Utc; }
        }
    }
}
