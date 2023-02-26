using System.Reflection;

namespace Camunda.Client;

public static class ZeebeWorkerAttributeExtensions
{
    public static ZeebeWorkerAttribute? GetZeebeWorkerAttribute(this Type input)
    {
        var result = input.GetCustomAttributes<ZeebeWorkerAttribute>().FirstOrDefault();
        return result;
    }
}
