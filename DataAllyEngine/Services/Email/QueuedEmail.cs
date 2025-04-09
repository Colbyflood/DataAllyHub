namespace DataAllyEngine.Services.Email;

public class QueuedEmail
{
	public string Sender { get; }
	public List<string> Recipients { get; } 
	public string Subject { get; }
	public string Body { get; }

	public QueuedEmail(string sender, List<string> recipients, string subject, string body)
	{
		Sender = sender;
		Recipients = recipients;
		Subject = subject;
		Body = body;
	}
}