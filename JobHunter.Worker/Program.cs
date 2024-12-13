using JobHunter.Application;
using JobHunter.Infrastructure.Persistent.Postgres;
using JobHunter.Application.Services;
using JobHunter.Domain.Job.Configurations;
using JobHunter.Domain.Job.Services;
using JobHunter.Framework.Observability.OpenTelemetry;
using JobHunter.Infrastructure.Analyzer.OpenAi;
using JobHunter.Infrastructure.Analyzer.OpenAi.Configurations;
using JobHunter.Infrastructure.Cache.Redis;
using JobHunter.Infrastructure.Linkedin;
using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Worker;
using JobHunter.Worker.Jobs;
using OpenTelemetry.Trace;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

var telemetryConfig = new ObservabilityConfig();
builder.Configuration.GetSection(nameof(ObservabilityConfig)).Bind(telemetryConfig);
builder.Services.AddSingleton(telemetryConfig);
builder.Services.AddOpenTelemetryServices(telemetryConfig);

var linkedinConfiguration = new LinkedinConfiguration();
builder.Configuration.GetSection(nameof(LinkedinConfiguration)).Bind(linkedinConfiguration);
builder.Services.AddSingleton(linkedinConfiguration);

var backgroundJobConfigurations = new BackgroundJobConfigurations();
builder.Configuration.GetSection(nameof(BackgroundJobConfigurations)).Bind(backgroundJobConfigurations);
builder.Services.AddSingleton(backgroundJobConfigurations);

var playwrightConfigurations = new PlaywrightConfigurations();
builder.Configuration.GetSection(nameof(PlaywrightConfigurations)).Bind(playwrightConfigurations);
builder.Services.AddSingleton(playwrightConfigurations);

var openAiConfigurations = new OpenAiConfigurations();
builder.Configuration.GetSection(nameof(OpenAiConfigurations)).Bind(openAiConfigurations);

builder.Services.AddCrawlerService()
    .AddOpenAiService(openAiConfigurations)
    .AddAutoMapperService();

var redisConfiguration = new RedisConfigurations();
builder.Configuration.GetSection(nameof(RedisConfigurations)).Bind(redisConfiguration);
builder.Services.AddRedisCache(redisConfiguration);

var connectionString = builder.Configuration.GetConnectionString("JobHunter");
builder.Services.AddRepositories()
    .AddServices()
    .AddJobHunterDbContext(connectionString);

builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();
builder.Services.AddScoped<IJobAnalysisBackgroundService, JobAnalysisBackgroundService>();
builder.Services.AddSingleton(TracerProvider.Default.GetTracer(telemetryConfig.ServiceName));


builder.Services.AddQuartz(q =>
{
    // Just use the name of your job that you created in the Jobs folder.
    var jobKey = new JobKey("JobCrawling");
    q.AddJob<BackgroundJobWorker>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("JobCrawling-trigger")
        //This Cron interval can be described as "run every minute" (when second is zero)
        .WithSimpleSchedule(x => x
            .RepeatForever()
            .WithIntervalInMinutes(backgroundJobConfigurations.CrawlerRepeatInterval)
        )
    );
});

builder.Services.AddQuartz(q =>
{
    // Just use the name of your job that you created in the Jobs folder.
    var jobKey = new JobKey("JobAnalyzer");
    q.AddJob<AnalyzerBackgroundJobWorker>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("JobAnalyzer-trigger")
        //This Cron interval can be described as "run every minute" (when second is zero)
        .WithSimpleSchedule(x => x
            .RepeatForever()
            .WithIntervalInMinutes(backgroundJobConfigurations.AnalyzerRepeatInterval)
        )
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Host.UseOpenTelemetryServices(telemetryConfig);
var app = builder.Build();

var migrator = new DbMigrator(app.Services.GetRequiredService<IServiceScopeFactory>());
await migrator.StartAsync(app.Lifetime.ApplicationStopping);
await app.RunAsync();