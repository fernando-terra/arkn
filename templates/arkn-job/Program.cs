using Arkn.Jobs.Extensions;
using Arkn.Logging.Extensions;
using ArknJob.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddArknLogging(logging =>
{
    logging.AddConsoleSink();
    logging.AddInMemorySink();
});

builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<SampleJob>("* * * * *")
        .WithName("sample-job")
        .WithDescription("Runs every minute — replace with your logic")
        .WithTimeout(TimeSpan.FromSeconds(30))
        .WithRetry(maxAttempts: 2);
});

var host = builder.Build();
host.Run();
