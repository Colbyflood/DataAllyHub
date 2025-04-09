namespace DataAllyEngine.Services.Email;

public class EmailQueueProcessingService : IEmailQueueProcessingService
{
	private const int PROCESSING_TIMEOUT_MSEC = 15 * 1000;
	
	private readonly IEmailQueueContainer emailQueueContainer;
	private readonly IEmailSender emailSender;
	private readonly ILogger<EmailQueueProcessingService> logger;
	
	public EmailQueueProcessingService(IEmailQueueContainer emailQueueContainer, IEmailSender emailSender, ILogger<EmailQueueProcessingService> logger)
	{
		this.emailQueueContainer = emailQueueContainer;
		this.emailSender = emailSender;
		this.logger = logger;
	}

	public async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation("{ServiceName} working", nameof(EmailQueueProcessingService));

		while (!stoppingToken.IsCancellationRequested)
		{
			if (!ProcessEmailQueue())
			{
				await Task.Delay(PROCESSING_TIMEOUT_MSEC, stoppingToken);
			}
		}
	}

	private bool ProcessEmailQueue()
	{
		try
		{
			var queuedEmail = emailQueueContainer.NextEmail();
			if (queuedEmail != null)
			{
				logger.LogInformation($"{nameof(EmailQueueProcessingService)} has received a queued message to transmit.");
				SendQueuedMessage(queuedEmail);
				return true;
			}
		}
		catch (Exception ex)
		{
			logger.LogError($"Error processing Email queue: {ex}");
		}

		return false;
	}

	private void SendQueuedMessage(QueuedEmail queuedEmail)
	{
		try
		{
			emailSender.SendMailToMultipleRecipients(queuedEmail.Sender, queuedEmail.Recipients, queuedEmail.Subject, queuedEmail.Body);
		}
		catch (Exception ex)
		{
			logger.LogError($"Error sending Email with subject {queuedEmail.Subject}: {ex}");
		}		
	}
}