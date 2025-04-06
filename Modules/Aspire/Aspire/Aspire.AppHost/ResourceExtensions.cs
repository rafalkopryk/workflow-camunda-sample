public static class ResourceExtensions
{
    public static T GetParameter<T>(this IDistributedApplicationBuilder builder, string name) where T : IParsable<T>
    {
        var param = builder.AddParameter(name);
        return T.Parse(param.Resource.Value, null);
    }
}