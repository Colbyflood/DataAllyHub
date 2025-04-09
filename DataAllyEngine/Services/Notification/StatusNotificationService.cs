namespace DataAllyEngine.Services.Notification;

public class StatusNotificationService : IStatusNotificationService
{
	private readonly IServiceProxy serviceProxy;
	private readonly IEmailQueueService emailQueueService;
	private readonly ILogger<StatusNotificationService> logger;

	public StatusNotificationService(IServiceProxy serviceProxy, IEmailQueueService emailQueueService, ILogger<StatusNotificationService> logger)
	{
		this.serviceProxy = serviceProxy;
		this.emailQueueService = emailQueueService;
		this.logger = logger;
	}
	
	public void SendServiceStartupStatus(string serviceName)
	{
		try
		{
			var recipients = serviceProxy.GetCriticalEmailRecipients();
			if (recipients.Count == 0)
			{
				logger.LogCritical($"Critical Notification Email Recipients are not configured!!");
				return;
			}

			var subject = $"Notice: MakeMyCap {serviceName} service started";
			var body = $"This is just a notification to alert you to the fact that the {serviceName} service on the MakeMyCap server has started up.\r\n\r\nThere is nothing to do unless this startup was unexpected.";
			
			emailQueueService.Add(recipients, subject, body);
		}
		catch (Exception ex)
		{
			logger.LogCritical($"Error emailing Start Service Notification for {serviceName} because of {ex}");
		}
	}

	public void SendServiceShutdownStatus(string serviceName)
	{
		try
		{
			var recipients = serviceProxy.GetCriticalEmailRecipients();
			if (recipients.Count == 0)
			{
				logger.LogCritical($"Critical Notification Email Recipients are not configured!!");
				return;
			}

			var subject = $"Notice: MakeMyCap {serviceName} service stopped";
			var body = $"This is just a notification to alert you to the fact that the {serviceName} service on the MakeMyCap server has stopped/shut down.\r\n\r\nThere is nothing to do unless this shutdown was unexpected or is not followed by a startup notification.";
			
			emailQueueService.Add(recipients, subject, body);
		}
		catch (Exception ex)
		{
			logger.LogCritical($"Error emailing Stop Service Notification for {serviceName} because of {ex}");
		}
	}
}