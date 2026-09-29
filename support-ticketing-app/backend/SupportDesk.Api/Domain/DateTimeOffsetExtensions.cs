namespace SupportDesk.Api.Domain;

public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset AddBusinessHours(this DateTimeOffset start, int hours)
    {
        var current = start;
        var remaining = hours;

        while (remaining > 0)
        {
            current = current.AddHours(1);

            if (current.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday &&
                current.Hour is >= 9 and < 17)
            {
                remaining--;
            }
        }

        return current;
    }
}
