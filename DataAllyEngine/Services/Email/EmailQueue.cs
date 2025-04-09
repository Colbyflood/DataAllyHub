using System.Collections.Concurrent;

namespace DataAllyEngine.Services.Email;

public class EmailQueueContainer : IEmailQueueContainer
{
	private ConcurrentQueue<QueuedEmail> emailQueue = new ConcurrentQueue<QueuedEmail>();

	public void InsertEmail(QueuedEmail data)
	{
		emailQueue.Enqueue(data);
	}

	public QueuedEmail? NextEmail()
	{
		return emailQueue.TryDequeue(out var data) ? data : null;
	}
}