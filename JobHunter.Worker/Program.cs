using JobHunter.Infrastructure.Persistent.Postgres;
using JobHunter.Application.Services;
using JobHunter.Domain.Job.Configurations;
using JobHunter.Domain.Job.Services;
using JobHunter.Infrastructure.Linkedin;
using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Worker.Jobs;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

var linkedinConfiguration = new LinkedinConfiguration();
builder.Configuration.GetSection(nameof(LinkedinConfiguration)).Bind(linkedinConfiguration);
builder.Services.AddSingleton(linkedinConfiguration);

var backgroundJobConfigurations = new BackgroundJobConfigurations();
builder.Configuration.GetSection(nameof(BackgroundJobConfigurations)).Bind(backgroundJobConfigurations);
builder.Services.AddSingleton(backgroundJobConfigurations);

var userBackgroundJobSettings = new UserBackgroundJobSettings();
builder.Configuration.GetSection(nameof(UserBackgroundJobSettings)).Bind(userBackgroundJobSettings);
builder.Services.AddSingleton(userBackgroundJobSettings);

builder.Services.AddCrawlerService()
    .AddLinkedinHttpClient(linkedinConfiguration);

var connectionString = builder.Configuration.GetConnectionString("JobHunter");
builder.Services.AddRepositories()
    .AddJobHunterDbContext(connectionString);

builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();


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
            .WithIntervalInMinutes(backgroundJobConfigurations.RepeatInterval)
        )
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();


await app.RunAsync();