using DataAllyEngine.Services.Notification;

namespace DataAllyEngine.Services.Email;

public class EmailSendingScopedBackgroundService : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<EmailSendingScopedBackgroundService> logger;
	
	public EmailSendingScopedBackgroundService(IServiceProvider serviceProvider, ILogger<EmailSendingScopedBackgroundService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(EmailSendingScopedBackgroundService)} is running.");
		await DoWorkAsync(stoppingToken);
	}

	private async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation($"{nameof(EmailSendingScopedBackgroundService)} is working.");


		using (IServiceScope scope = serviceProvider.CreateScope())
		{
			IStatusNotificationService statusNotificationService = scope.ServiceProvider.GetRequiredService<IStatusNotificationService>();
			statusNotificationService.SendServiceStartupStatus(nameof(EmailSendingScopedBackgroundService));

			IEmailQueueProcessingService scopedQueueProcessingService = scope.ServiceProvider.GetRequiredService<IEmailQueueProcessingService>();
			await scopedQueueProcessingService.DoWorkAsync(stoppingToken);
			
			statusNotificationService.SendServiceShutdownStatus(nameof(EmailSendingScopedBackgroundService));
		}
		
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation($"{nameof(EmailSendingScopedBackgroundService)} is stopping.");
		await base.StopAsync(stoppingToken);
	}	
}