namespace DataAllyEngine.Services.Email;

public interface IEmailQueueContainer
{
	void InsertEmail(QueuedEmail data);
	QueuedEmail? NextEmail();
}