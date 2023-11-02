public class QueryLuceneBuilder
{
    private readonly List<string> _query = new();

    public QueryLuceneBuilder Append(string key, long? value)
    {
        if(value is null)
        {
            return this;
        }

        _query.Add($"{key}: {value}");

        return this;
    }

    public QueryLuceneBuilder Append(string key, string? value)
    {
        if (value is null)
        {
            return this;
        }

        _query.Add($"{key}: \"{value}\"");

        return this;
    }

    public string Build()
    {
        return _query.Any()
            ? string.Join(" AND ", _query)
            : string.Empty;
    }
}
