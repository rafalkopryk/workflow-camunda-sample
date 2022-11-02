using System.Reflection;

namespace Common.Application.Zeebe
{
    public static class ZeebeJobExtensions
    {
        public static ZeebeJobAttribute? GetZeebeJobAttribute(this Type input)
        {
            var result = input.GetCustomAttributes<ZeebeJobAttribute>().FirstOrDefault();
            return result;
        }
    }
}
