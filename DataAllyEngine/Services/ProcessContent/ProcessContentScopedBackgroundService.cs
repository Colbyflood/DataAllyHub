using DataAllyEngine.Services.Notification;

namespace DataAllyEngine.Services.ProcessContent;

public class ProcessContentScopedBackgroundService : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<ProcessContentScopedBackgroundService> logger;
	
	public ProcessContentScopedBackgroundService(IServiceProvider serviceProvider, ILogger<ProcessContentScopedBackgroundService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(ProcessContentScopedBackgroundService)} is running.");
		await DoWorkAsync(stoppingToken);
	}

	private async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation($"{nameof(ProcessContentScopedBackgroundService)} is working.");

		using (IServiceScope scope = serviceProvider.CreateScope())
		{
			IStatusNotificationService statusNotificationService = scope.ServiceProvider.GetRequiredService<IStatusNotificationService>();
			statusNotificationService.SendServiceStartupStatus(nameof(ProcessContentScopedBackgroundService));

			IProcessContentService scopedProcessingService = scope.ServiceProvider.GetRequiredService<IProcessContentService>();
			await scopedProcessingService.DoWorkAsync(stoppingToken);

			statusNotificationService.SendServiceShutdownStatus(nameof(ProcessContentScopedBackgroundService));
		}
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(ProcessContentScopedBackgroundService)} is stopping.");
		await base.StopAsync(stoppingToken);
	}	
}