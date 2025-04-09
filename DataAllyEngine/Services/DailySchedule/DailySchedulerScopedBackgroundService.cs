namespace DataAllyEngine.Services.DailySchedule;

public class DailySchedulerScopedBackgroundService : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<DailySchedulerScopedBackgroundService> logger;
	
	public DailySchedulerScopedBackgroundService(IServiceProvider serviceProvider, ILogger<DailySchedulerScopedBackgroundService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(DailySchedulerScopedBackgroundService)} is running.");
		await DoWorkAsync(stoppingToken);
	}

	private async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation($"{nameof(DailySchedulerScopedBackgroundService)} is working.");

		using (IServiceScope scope = serviceProvider.CreateScope())
		{
			IStatusNotificationService statusNotificationService = scope.ServiceProvider.GetRequiredService<IStatusNotificationService>();
			statusNotificationService.SendServiceStartupStatus(nameof(DailySchedulerScopedBackgroundService));

			IDailySchedulerService scopedProcessingService = scope.ServiceProvider.GetRequiredService<IDailySchedulerService>();
			await scopedProcessingService.DoWorkAsync(stoppingToken);

			statusNotificationService.SendServiceShutdownStatus(nameof(DailySchedulerScopedBackgroundService));
		}
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(DailySchedulerScopedBackgroundService)} is stopping.");
		await base.StopAsync(stoppingToken);
	}	
}