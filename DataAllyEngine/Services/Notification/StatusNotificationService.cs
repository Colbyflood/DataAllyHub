using DataAllyEngine.Services.Email;

namespace DataAllyEngine.Services.Notification;

public class StatusNotificationService : IStatusNotificationService
{
	private readonly IEmailQueueService emailQueueService;
	private readonly ILogger<StatusNotificationService> logger;

	public StatusNotificationService(IEmailQueueService emailQueueService, ILogger<StatusNotificationService> logger)
	{
		this.emailQueueService = emailQueueService;
		this.logger = logger;
	}

	private List<string> GetRecipients()
	{
		// TODO: configure notification recipients
		return ["support@dataally.ai", "786hammadsajid+dataally@gmail.com"];
	}


	public void SendServiceStartupStatus(string serviceName)
	{
		try
		{
			var subject = $"Notice: DataAllyHub {serviceName} service started";
			var body = $"This is just a notification to alert you to the fact that the {serviceName} service on the DataAllyHub server has started up.\r\n\r\nThere is nothing to do unless this startup was unexpected.";
			
			emailQueueService.Add(GetRecipients(), subject, body);
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
			var subject = $"Notice: DataAllyHub {serviceName} service stopped";
			var body = $"This is just a notification to alert you to the fact that the {serviceName} service on the DataAllyHub server has stopped/shut down.\r\n\r\nThere is nothing to do unless this shutdown was unexpected or is not followed by a startup notification.";
			
			emailQueueService.Add(GetRecipients(), subject, body);
		}
		catch (Exception ex)
		{
			logger.LogCritical($"Error emailing Stop Service Notification for {serviceName} because of {ex}");
		}
	}
}