using DataAllyEngine.Configuration;

namespace DataAllyEngine.Services.Email;

public class EmailQueueService : IEmailQueueService
{
	// ReSharper disable once MemberCanBePrivate.Global
	// ReSharper disable once InconsistentNaming
	public const string EMAIL_SENDER = "EmailSender";

	private readonly string defaultSender;
	private readonly IEmailQueueContainer emailQueueContainer;
	private readonly ILogger<EmailQueueService> logger;
	
	public EmailQueueService(IConfigurationLoader configurationLoader, IEmailQueueContainer emailQueueContainer, ILogger<EmailQueueService> logger)
	{
		this.emailQueueContainer = emailQueueContainer;
		this.logger = logger;
		defaultSender = configurationLoader.GetKeyValueFor(EMAIL_SENDER);
	}
	
	public void Add(string recipient, string subject, string body, string? sender = null)
	{
		Add([recipient], subject, body, sender);
	}

	public void Add(List<string> recipients, string subject, string body, string? sender = null)
	{
		emailQueueContainer.InsertEmail(new QueuedEmail(string.IsNullOrEmpty(sender) ? sender : defaultSender, recipients, subject, body));
	}
}