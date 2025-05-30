using DataAllyEngine.Services.Notification;

namespace DataAllyEngine.Services.CreativeImagesLoader;

public class CreativeImagesLoaderScopedBackgroundService : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<CreativeImagesLoaderScopedBackgroundService> logger;
	
	public CreativeImagesLoaderScopedBackgroundService(IServiceProvider serviceProvider, ILogger<CreativeImagesLoaderScopedBackgroundService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(CreativeImagesLoaderScopedBackgroundService)} is running.");
		await DoWorkAsync(stoppingToken);
	}

	private async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation($"{nameof(CreativeImagesLoaderScopedBackgroundService)} is working.");

		using (IServiceScope scope = serviceProvider.CreateScope())
		{
			IStatusNotificationService statusNotificationService = scope.ServiceProvider.GetRequiredService<IStatusNotificationService>();
			statusNotificationService.SendServiceStartupStatus(nameof(CreativeImagesLoaderScopedBackgroundService));

			ICreativeImagesLoadingService scopedProcessingService = scope.ServiceProvider.GetRequiredService<ICreativeImagesLoadingService>();
			await scopedProcessingService.DoWorkAsync(stoppingToken);

			statusNotificationService.SendServiceShutdownStatus(nameof(CreativeImagesLoaderScopedBackgroundService));
		}
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(CreativeImagesLoaderScopedBackgroundService)} is stopping.");
		await base.StopAsync(stoppingToken);
	}	
}