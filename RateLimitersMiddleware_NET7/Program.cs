using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace RateLimitersMiddleware_NET7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Throttle the thread pool (set available threads to amount of processors)
            //ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
            
            builder.Services.AddControllers();
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 1000,
                            QueueLimit = 100,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddFixedWindowLimiter("FixedWindow", options =>
                {
                    options.AutoReplenishment = true;
                    options.PermitLimit = 1000;
                    options.QueueLimit = 100;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.Window = TimeSpan.FromMinutes(1);
                });
                options.AddSlidingWindowLimiter("SlidingWindow", options =>
                {
                    options.AutoReplenishment = true;
                    options.PermitLimit = 1000;
                    options.QueueLimit = 100;
                    options.SegmentsPerWindow = 1;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.Window = TimeSpan.FromMinutes(1);
                });
                options.AddConcurrencyLimiter("Concurrency", options =>
                {
                    options.PermitLimit = 1000;
                    options.QueueLimit = 100;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync(
                            $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s). " +
                            $"Read more about our rate limits at https://example.org/docs/ratelimiting.", cancellationToken: token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync(
                            "Too many requests. Please try again later. " +
                            "Read more about our rate limits at https://example.org/docs/ratelimiting.", cancellationToken: token);
                    }
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseRateLimiter();

            app.MapControllers();

            app.Run();
        }
    }
}