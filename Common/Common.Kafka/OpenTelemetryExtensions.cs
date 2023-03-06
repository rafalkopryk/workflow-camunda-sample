namespace Common.Kafka;
using System.Diagnostics;
using System.Diagnostics.Metrics;


internal static class OpenTelemetryExtensions
{
    public static void Add(this Counter<int> counter, params (string key, string value)[] tags)
    {
        var tagList = new TagList();
        foreach (var tag in tags)
        {
            tagList.Add(tag.key, tag.value);
        }

        counter.Add(1, tagList);
    }

    public static void Record(this Histogram<double> histogram, double value, params (string key, string value)[] tags)
    {
        var tagList = new TagList();
        foreach (var tag in tags)
        {
            tagList.Add(tag.key, tag.value);
        }

        histogram.Record(value, tagList);
    }
}
