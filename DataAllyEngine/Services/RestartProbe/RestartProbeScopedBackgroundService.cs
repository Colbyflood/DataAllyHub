using DataAllyEngine.Services.Notification;

namespace DataAllyEngine.Services.RestartProbe;

public class RestartProbeScopedBackgroundService : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<RestartProbeScopedBackgroundService> logger;
	
	public RestartProbeScopedBackgroundService(IServiceProvider serviceProvider, ILogger<RestartProbeScopedBackgroundService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(RestartProbeScopedBackgroundService)} is running.");
		await DoWorkAsync(stoppingToken);
	}

	private async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation($"{nameof(RestartProbeScopedBackgroundService)} is working.");

		using (IServiceScope scope = serviceProvider.CreateScope())
		{
			IStatusNotificationService statusNotificationService = scope.ServiceProvider.GetRequiredService<IStatusNotificationService>();
			statusNotificationService.SendServiceStartupStatus(nameof(RestartProbeScopedBackgroundService));

			IRestartProbeService scopedProcessingService = scope.ServiceProvider.GetRequiredService<IRestartProbeService>();
			await scopedProcessingService.DoWorkAsync(stoppingToken);

			statusNotificationService.SendServiceShutdownStatus(nameof(RestartProbeScopedBackgroundService));
		}
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(RestartProbeScopedBackgroundService)} is stopping.");
		await base.StopAsync(stoppingToken);
	}	
}