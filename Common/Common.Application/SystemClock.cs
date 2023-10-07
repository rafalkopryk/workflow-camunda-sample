public static class SystemClock
{
    private static DateTimeOffset? _customDate;

    public static DateTimeOffset Now => _customDate.HasValue ? _customDate.Value : DateTimeOffset.Now;

    public static void Set(DateTimeOffset customDate) => _customDate = customDate;

    public static void Reset() => _customDate = null;
}