public static class ResourceExtensions
{
    public static T GetParameter<T>(this IDistributedApplicationBuilder builder, string name) where T : IParsable<T>
    {
        var existing = builder.Resources
            .OfType<ParameterResource>()
            .FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        var value = existing is not null ? existing.Value : builder.AddParameter(name).Resource.Value;
        return T.Parse(value, null);
    }
}