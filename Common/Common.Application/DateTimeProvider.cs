namespace Common.Application;

public sealed class DateTimeProvider : TimeProvider
{
    private DateTimeProvider()
    {
    }

    private static DateTimeOffset? _customUtc;

    public override DateTimeOffset GetUtcNow() => _customUtc ?? TimeProvider.System.GetUtcNow();

    public static readonly TimeProvider Shared = new DateTimeProvider();

    public static void SetUtc(DateTimeOffset customDate) => _customUtc = customDate;

    public static void Reset() => _customUtc = null;
}