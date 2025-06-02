using DataAllyEngine.Models;
using DataAllyEngine.Proxy;

namespace DataAllyEngine.Services.CreativeLoader;

public abstract class AbstractCreativeLoader
{
	// ReSharper disable InconsistentNaming
	protected const int ONE_MINUTE = 1;
	protected const int ONE_MINUTE_MSEC = ONE_MINUTE * 60 * 1000;

	protected const int FIVE_MINUTES = 5;
	protected const int FIVE_MINUTES_MSEC = FIVE_MINUTES * 60 * 1000;
	
	protected const int ONE_HOUR_MINUTES = 60;
	protected const int RETRY_TIMEOUT_MINUTES = ONE_HOUR_MINUTES;
	protected const int RETRY_TIMEOUT_MSEC = RETRY_TIMEOUT_MINUTES * 60 * 1000;
	
	protected readonly ILoaderProxy loaderProxy;
	protected readonly ISchedulerProxy schedulerProxy;
	protected readonly ITokenHolder tokenHolder;

	private string serviceName;
	private ILogger logger;

	protected AbstractCreativeLoader(string serviceName, ILoaderProxy loaderProxy, 
		ISchedulerProxy schedulerProxy, ITokenHolder tokenHolder, ILogger logger)
	{
		this.serviceName = serviceName;
		this.loaderProxy = loaderProxy;
		this.schedulerProxy = schedulerProxy;
		this.tokenHolder = tokenHolder;
		this.logger = logger;
	}
	
	public async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation("{ServiceName} working", serviceName);

		while (!stoppingToken.IsCancellationRequested)
		{
			if (CheckAndProcessPendingContent())
			{
				await Task.Delay(FIVE_MINUTES_MSEC, stoppingToken);
			}
		}
	}
	
	protected bool CheckAndProcessPendingContent()
	{
		var now = DateTime.UtcNow;

		// get list of next 1000 pending creatives to load
		var batch = GetNextPendingCreativesBatch();
		
		// if list is empty, return true to indicate no pending content to process
		if (batch.Count == 0)
		{
			return true;
		}

		foreach (var element in batch)
		{
			ProcessCreative(element);
		}

		return false;
	}
	
	// Need a common way to share tokens acquired to load images/videos by companyId by all implementations
	
	protected abstract List<FbCreativeLoad> GetNextPendingCreativesBatch();
	
	protected abstract void ProcessCreative(FbCreativeLoad creative);
}