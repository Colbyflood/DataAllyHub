using DataAllyEngine.Configuration;

namespace DataAllyEngine.Services.Email;

public class EmailQueueService : IEmailQueueService
{
	public const string SENDGRID_SENDER = "SendGridSender";

	private readonly IEmailProxy emailProxy;
	private readonly string defaultSender;
	private readonly ILogger<EmailQueueService> logger;
	
	public EmailQueueService(IConfigurationLoader configurationLoader, IEmailProxy emailProxy, ILogger<EmailQueueService> logger)
	{
		this.emailProxy = emailProxy;
		this.logger = logger;
		
		defaultSender = configurationLoader.GetKeyValueFor(SENDGRID_SENDER);
	}
	
	public void Add(string recipient, string subject, string body, string sender = null)
	{
		var recipients = new List<string>();
		recipients.Add(recipient);
		Add(recipients, subject, body, sender);
	}

	public void Add(List<string> recipients, string subject, string body, string sender = null)
	{
		emailProxy.QueueMessage(string.IsNullOrEmpty(sender) ? defaultSender : sender, recipients, subject, body);
	}
}