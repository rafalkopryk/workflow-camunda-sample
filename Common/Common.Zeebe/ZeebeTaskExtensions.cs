using System.Reflection;

namespace Common.Application.Zeebe;

public static class ZeebeTaskExtensions
{
    public static ZeebeTaskAttribute? GetZeebeTaskAttribute(this Type input)
    {
        var result = input.GetCustomAttributes<ZeebeTaskAttribute>().FirstOrDefault();
        return result;
    }
}
