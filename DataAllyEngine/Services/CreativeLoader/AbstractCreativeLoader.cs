namespace DataAllyEngine.Services.CreativeLoader;

public abstract class AbstractCreativeLoader
{
	// ReSharper disable InconsistentNaming
	protected const int ONE_MINUTE = 1;
	protected const int ONE_MINUTE_MSEC = ONE_MINUTE * 60 * 1000;

	protected const int FIVE_MINUTES = 5;
	protected const int FIVE_MINUTES_MSEC = FIVE_MINUTES * 60 * 1000;
	
	private string serviceName;
	private ILogger logger;

	protected AbstractCreativeLoader(string serviceName, ILogger logger)
	{
		this.serviceName = serviceName;
		this.logger = logger;
	}
	
	public async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation("{ServiceName} working", serviceName);

		while (!stoppingToken.IsCancellationRequested)
		{
			CheckAndProcessPendingContent();
			await Task.Delay(FIVE_MINUTES_MSEC, stoppingToken);
		}
	}
	
	protected void CheckAndProcessPendingContent()
	{
		var now = DateTime.UtcNow;

		// What to do here....???
		// delegate where needed to derived classes
	}
	
	// Need a common way to share tokens acquired to load images/videos by companyId by all implementations
}