using DataAllyEngine.Services.CreativeImagesLoader;
using DataAllyEngine.Services.Notification;

namespace DataAllyEngine.Services.CreativeVideosLoader;

public class CreativeVideosLoaderScopedBackgroundService : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<CreativeVideosLoaderScopedBackgroundService> logger;
	
	public CreativeVideosLoaderScopedBackgroundService(IServiceProvider serviceProvider, ILogger<CreativeVideosLoaderScopedBackgroundService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(CreativeVideosLoaderScopedBackgroundService)} is running.");
		await DoWorkAsync(stoppingToken);
	}

	private async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation($"{nameof(CreativeVideosLoaderScopedBackgroundService)} is working.");

		using (IServiceScope scope = serviceProvider.CreateScope())
		{
			IStatusNotificationService statusNotificationService = scope.ServiceProvider.GetRequiredService<IStatusNotificationService>();
			statusNotificationService.SendServiceStartupStatus(nameof(CreativeVideosLoaderScopedBackgroundService));

			ICreativeVideosLoadingService scopedProcessingService = scope.ServiceProvider.GetRequiredService<ICreativeVideosLoadingService>();
			await scopedProcessingService.DoWorkAsync(stoppingToken);

			statusNotificationService.SendServiceShutdownStatus(nameof(CreativeVideosLoaderScopedBackgroundService));
		}
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(CreativeVideosLoaderScopedBackgroundService)} is stopping.");
		await base.StopAsync(stoppingToken);
	}	
}