using Amazon.S3;
using Amazon.SimpleEmail;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;
using DataAllyEngine.ContentProcessingTask;
using DataAllyEngine.Context;
using DataAllyEngine.LoaderTask;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.DailySchedule;
using DataAllyEngine.Services.Email;
using DataAllyEngine.Services.Notification;
using DataAllyEngine.Services.ProcessContent;
using DataAllyEngine.Services.RestartProbe;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var configurationPath = builder.Configuration.GetValue<string>("XmlConfiguration");
var xmlConfigurationLoader = new XmlConfigurationLoader(configurationPath);

// Add services to the container.
builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();
builder.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

builder.Services.Add(new ServiceDescriptor(typeof(IConfigurationLoader), xmlConfigurationLoader));

// create db contexts for each of the domains
builder.Services.AddDbContext<DataAllyDbContext>(options =>
    options.UseMySQL(xmlConfigurationLoader.GetKeyValueFor(Names.DB_CONNECTION_STRING_KEY)), ServiceLifetime.Scoped);


// Add singleton injectables
builder.Services.AddSingleton<IEmailQueueContainer, EmailQueueContainer>();


// Add injectable proxies
builder.Services.AddScoped<ILoaderProxy, LoaderProxy>();
builder.Services.AddScoped<ISchedulerProxy, SchedulerProxy>();
builder.Services.AddScoped<IContentProcessorProxy, ContentProcessorProxy>();
builder.Services.AddScoped<IKpiProxy, KpiProxy>();

//if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<IAmazonSimpleEmailService>();
    builder.Services.AddAWSService<IAmazonS3>();
}


// Add services
builder.Services.AddScoped<ILoaderRunner, LoaderRunner>();
builder.Services.AddScoped<IContentProcessor, ContentProcessor>();
builder.Services.AddScoped<IEmailSender, AmazonSesEmailSender>();
builder.Services.AddScoped<IEmailQueueService, EmailQueueService>();
builder.Services.AddScoped<IEmailQueueProcessingService, EmailQueueProcessingService>();
builder.Services.AddScoped<IStatusNotificationService, StatusNotificationService>();
builder.Services.AddScoped<IDailySchedulerService, DailySchedulerService>();
builder.Services.AddScoped<IRestartProbeService, RestartProbeService>();
builder.Services.AddScoped<IProcessContentService, ProcessContentService>();


// Add background services
builder.Services.AddHostedService<EmailSendingScopedBackgroundService>();
builder.Services.AddHostedService<DailySchedulerScopedBackgroundService>();
builder.Services.AddHostedService<RestartProbeScopedBackgroundService>();
builder.Services.AddHostedService<ProcessContentScopedBackgroundService>();


// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
