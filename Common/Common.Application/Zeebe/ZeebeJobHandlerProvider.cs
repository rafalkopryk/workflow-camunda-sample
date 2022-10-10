namespace Common.Application.Zeebe;
using System;
using System.Collections.Generic;
using System.Linq;

internal interface IZeebeJobHandlerProvider
{
    Type GetByKey(string key);

    (string Key, Type Type)[] GetJobs();
}

internal class ZeebeJobHandlerProvider : IZeebeJobHandlerProvider
{
    private readonly IList<(string Key, Type Type)> _registeredJobs = new List<(string, Type)>();

    public void RegisterZeebeJobs()
    {
        var jobs = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(x => typeof(IZeebeJob).IsAssignableFrom(x) && !x.IsInterface))
            .ToArray();

        foreach (var job in jobs)
        {
            if (!typeof(IZeebeJob).IsAssignableFrom(job))
                continue;

            var key = job.GetZeebeJobAttribute()?.JobType ?? throw new ArgumentNullException(nameof(ZeebeJobAttribute));
            if (_registeredJobs.Any(x => x.Key == key))
                continue;

            _registeredJobs.Add((key, job));
        }
    }

    public Type GetByKey(string key)
    {
        return _registeredJobs
            .FirstOrDefault(x => x.Key == key)
            .Type;
    }

    public (string Key, Type Type)[] GetJobs()
    {
        return _registeredJobs.ToArray();
    }
}
