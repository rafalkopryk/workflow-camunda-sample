#!/usr/bin/env dotnet
#:property TargetFramework=net10.0

using System.Diagnostics;
using System.Net;

var baseUrl = args.Length > 0 ? args[0] : "https://localhost:7230";
var totalRequests = args.Length > 1 ? int.Parse(args[1]) : 100_000;
var parallelism = args.Length > 2 ? int.Parse(args[2]) : 18;

var handler = new SocketsHttpHandler
{
    SslOptions = { RemoteCertificateValidationCallback = (_, _, _, _) => true },
    MaxConnectionsPerServer = parallelism,
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
};

using var http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

Console.WriteLine($"Target:        {baseUrl}");
Console.WriteLine($"Requests:      {totalRequests:N0}");
Console.WriteLine($"Parallelism:   {parallelism}");
Console.WriteLine();

var success = 0;
var failed = 0;
var sw = Stopwatch.StartNew();
var startDate = new DateOnly(2025, 1, 1);

await Parallel.ForEachAsync(
    Enumerable.Range(0, totalRequests),
    new ParallelOptions { MaxDegreeOfParallelism = parallelism },
    async (i, ct) =>
    {
        var date = startDate.AddDays(i % 3650);
        try
        {
            using var resp = await http.PostAsync($"/weatherforecast/{date:yyyy-MM-dd}", content: null, ct);
            if (resp.StatusCode is HttpStatusCode.Accepted or HttpStatusCode.OK)
                Interlocked.Increment(ref success);
            else
                Interlocked.Increment(ref failed);
        }
        catch
        {
            Interlocked.Increment(ref failed);
        }

        var done = success + failed;
        if (done % 1000 == 0)
            Console.WriteLine($"{done,8:N0} / {totalRequests:N0}  ok={success:N0}  fail={failed:N0}  elapsed={sw.Elapsed:mm\\:ss}");
    });

sw.Stop();

Console.WriteLine();
Console.WriteLine($"Done in {sw.Elapsed:mm\\:ss\\.fff}");
Console.WriteLine($"  ok      = {success:N0}");
Console.WriteLine($"  failed  = {failed:N0}");
Console.WriteLine($"  rps     = {totalRequests / sw.Elapsed.TotalSeconds:N0}");
