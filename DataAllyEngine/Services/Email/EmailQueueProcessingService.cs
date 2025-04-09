namespace DataAllyEngine.Services.Email;

public class EmailQueueProcessingService : IEmailQueueProcessingService
{
	private const int ONE_MINUTE = 2;
	private const int PROCESSING_TIMEOUT_MSEC = ONE_MINUTE * 60 * 1000;
	
	private const int FAILURE_HOURS = 48;

	private readonly IEmailProxy emailProxy;
	private readonly IEmailSender emailSender;
	private readonly IServiceProxy serviceProxy;
	private readonly ILogger<EmailQueueProcessingService> logger;
	
	public EmailQueueProcessingService(IEmailProxy emailProxy, IServiceProxy serviceProxy, IEmailSender emailSender, ILogger<EmailQueueProcessingService> logger)
	{
		this.emailProxy = emailProxy;
		this.serviceProxy = serviceProxy;
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
		ServiceLog? serviceLog = null;
		try
		{
			serviceLog = serviceProxy.CreateServiceLogFor(nameof(EmailQueueProcessingService));

			var queuedMessages = emailProxy.GetPendingQueuedMessages();
			if (queuedMessages.Count > 0)
			{
				logger.LogInformation($"Email queue contains {queuedMessages.Count} message(s)");
				foreach (var queuedMessage in queuedMessages)
				{
					SendQueuedMessage(queuedMessage);
					emailProxy.SaveQueuedMessage(queuedMessage);
				}
			}

			serviceProxy.CloseServiceLogFor(serviceLog);
		}
		catch (Exception ex)
		{
			logger.LogError($"Error processing Email queue: {ex}");
			if (serviceLog != null)
			{
				serviceProxy.CloseServiceLogFor(serviceLog);
			}
		}

		return false;
	}

	private void SendQueuedMessage(EmailQueue queuedMessage)
	{
		try
		{
			queuedMessage.TotalAttempts++;
			queuedMessage.LastAttemptDateTime = DateTime.Now;

			var recipients = ExtractRecipientsFrom(queuedMessage);
			if (recipients.Count == 1)
			{
				emailSender.SendMail(queuedMessage.Sender, recipients[0], queuedMessage.Subject, queuedMessage.Body, queuedMessage.BodyIsHtml);
			}
			else
			{
				emailSender.SendMailToMultipleRecipients(queuedMessage.Sender, recipients, queuedMessage.Subject, queuedMessage.Body, queuedMessage.BodyIsHtml);
			}

			queuedMessage.SentDateTime = DateTime.Now;
		}
		catch (Exception ex)
		{
			logger.LogError($"Error sending Email with Id {queuedMessage.Id}: {ex}");
			
			var now = DateTime.Now;
			var difference = now.Subtract(queuedMessage.PostedDateTime);
			if (difference.TotalHours > FAILURE_HOURS)
			{
				logger.LogCritical($"Failure to send Email with Id {queuedMessage.Id} after {Convert.ToInt32(difference.TotalHours)} hours and {queuedMessage.TotalAttempts}: Marking as ABANDONED");
				queuedMessage.AbandonedDateTime = DateTime.Now;
			}
		}		
	}

	private List<string> ExtractRecipientsFrom(EmailQueue queuedMessage)
	{
		var recipients = new List<string>();
		if (!string.IsNullOrEmpty(queuedMessage.Recipient))
		{
			recipients.Add(queuedMessage.Recipient);
		}
		if (!string.IsNullOrEmpty(queuedMessage.Recipient2))
		{
			recipients.Add(queuedMessage.Recipient2);
		}
		if (!string.IsNullOrEmpty(queuedMessage.Recipient3))
		{
			recipients.Add(queuedMessage.Recipient3);
		}
		if (!string.IsNullOrEmpty(queuedMessage.Recipient4))
		{
			recipients.Add(queuedMessage.Recipient4);
		}

		return recipients;
	}
}