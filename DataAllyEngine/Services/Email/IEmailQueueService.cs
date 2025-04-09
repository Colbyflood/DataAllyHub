namespace DataAllyEngine.Services.Email;

public interface IEmailQueueService
{
	void Add(string recipient, string subject, string body, string sender = null);
	void Add(List<string> recipients, string subject, string body, string sender = null);
}