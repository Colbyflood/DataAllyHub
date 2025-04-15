using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;

namespace DataAllyEngine.Services.Email;

public class AmazonSesEmailSender: IEmailSender
{
	private readonly string emailSender;
	private readonly IAmazonSimpleEmailService client;
	private readonly ILogger<AmazonSesEmailSender> logger;
	
	public AmazonSesEmailSender(IAmazonSimpleEmailService ses, IConfigurationLoader configurationLoader, ILogger<AmazonSesEmailSender> logger)
	{
		emailSender = configurationLoader.GetKeyValueFor(Names.EMAIL_SENDER_KEY);
		client = ses;
		this.logger = logger;
	}

	public async Task SendMail(string? sender, string to, string subject, string content, bool isHtml = false)
	{
		await SendMailToMultipleRecipients(sender, new List<string> { to }, subject, content, isHtml); 
	}

	public async Task SendMailToMultipleRecipients(string? sender, List<string> to, string subject, string content, bool isHtml = false)
	{
		try
		{
			var sendRequest = new SendEmailRequest
			{
				Source = string.IsNullOrEmpty(sender) ? emailSender : sender,
				Destination = new Destination { ToAddresses = to },
				Message = new Message
				{
					Subject = new Content(subject),
					Body = new Body { Text = new Content(content) }
				}
			};
			
			await client.SendEmailAsync(sendRequest);
		}
		catch (Exception ex)
		{
			logger.LogError($"Sending mail had an error: {ex}");
		}
	}
}