using DataAllyEngine.Services.Notification;

namespace DataAllyEngine.Services.BackfillLauncher;

public class BackfillLauncherScopedBackgroundService : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<BackfillLauncherScopedBackgroundService> logger;
	
	public BackfillLauncherScopedBackgroundService(IServiceProvider serviceProvider, ILogger<BackfillLauncherScopedBackgroundService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(BackfillLauncherScopedBackgroundService)} is running.");
		await DoWorkAsync(stoppingToken);
	}

	private async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation($"{nameof(BackfillLauncherScopedBackgroundService)} is working.");

		using (IServiceScope scope = serviceProvider.CreateScope())
		{
			IStatusNotificationService statusNotificationService = scope.ServiceProvider.GetRequiredService<IStatusNotificationService>();
			statusNotificationService.SendServiceStartupStatus(nameof(BackfillLauncherScopedBackgroundService));

			IBackfillLauncherService scopedProcessingService = scope.ServiceProvider.GetRequiredService<IBackfillLauncherService>();
			await scopedProcessingService.DoWorkAsync(stoppingToken);

			statusNotificationService.SendServiceShutdownStatus(nameof(BackfillLauncherScopedBackgroundService));
		}
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(BackfillLauncherScopedBackgroundService)} is stopping.");
		await base.StopAsync(stoppingToken);
	}	
}