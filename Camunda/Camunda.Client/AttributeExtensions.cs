using System.Reflection;

namespace Camunda.Client;

public static class AttributeExtensions
{
    public static T? GetAttribute<T>(this Type input) where T : Attribute
    {
        var result = input.GetCustomAttributes<T>().FirstOrDefault();
        return result;
    }
}
