using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CancellationTokensInAsynchronousIO
{
    class Program
    {
        static async Task<int> Main()
        {
            var builder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddTransient<IQueryService, QueryService>();
                }).UseConsoleLifetime();

            var host = builder.Build();

            using var serviceScope = host.Services.CreateScope();
            {
                var services = serviceScope.ServiceProvider;

                var queryService = services.GetRequiredService<IQueryService>();

                await MeasureCancellationAccuracy(queryService);
            }

            return 0;
        }

        private static async Task MeasureCancellationAccuracy(IQueryService queryService)
        {
            const double timeLimitInMs = 200;
            const ushort iterations = 1000;

            var precisionResultsInMs = new List<double>();

            for (int i = 0; i < iterations; i++)
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeLimitInMs));

                var stopwatch = new Stopwatch();

                (bool, bool) result = (false, false);

                stopwatch.Start();

                try
                {
                    result = await queryService.GetCommentsAsync(cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                }
                catch (OperationCanceledException)
                {
                }

                stopwatch.Stop();

                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                precisionResultsInMs.Add(elapsedMilliseconds - timeLimitInMs);

                Console.WriteLine($"Second operation started: {result.Item1}, second operation succeeded: {result.Item2}. Elapsed time: {elapsedMilliseconds}ms.");
            }

            var bestPrecisionMs = precisionResultsInMs.Min();
            var bestPrecision = $"{(bestPrecisionMs < 0 ? 0 : bestPrecisionMs):0.00}ms";

            var worstPrecision = $"{precisionResultsInMs.Max():0.00}ms";

            var averagePrecisionMs = precisionResultsInMs.Average();
            var averagePrecision = $"{(averagePrecisionMs < 0 ? 0 : averagePrecisionMs):0.00}ms";

            Console.WriteLine(
                $"Summary:{Environment.NewLine}Best precision: {bestPrecision}{Environment.NewLine}Worst precision: {worstPrecision}{Environment.NewLine}Average precision: {averagePrecision}");
        }
    }
}