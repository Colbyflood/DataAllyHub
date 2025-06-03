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
	
	protected const int FIFTEEN_SEC = 15;
	protected const int FIFTEEN_SEC_MSEC = FIFTEEN_SEC * 1000;
	
	protected const int ONE_HOUR_MINUTES = 60;
	protected const int RETRY_TIMEOUT_MINUTES = ONE_HOUR_MINUTES;
	protected const int RETRY_TIMEOUT_MSEC = RETRY_TIMEOUT_MINUTES * 60 * 1000;
	
	protected const int BATCH_SIZE = 1000;
	
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

		int startId = 0;
		while (!stoppingToken.IsCancellationRequested)
		{
			
			if (CheckAndProcessPendingContent(startId, out startId))
			{
				await Task.Delay(FIVE_MINUTES_MSEC, stoppingToken);
				startId = 0;
			}
			else
			{
				await Task.Delay(FIFTEEN_SEC_MSEC, stoppingToken);
			}
		}
	}
	
	protected bool CheckAndProcessPendingContent(int startId, out int nextId)
	{
		var now = DateTime.UtcNow;
		nextId = startId;

		var batch = GetNextPendingCreativesBatch(startId, BATCH_SIZE);
		
		if (batch.Count == 0)
		{
			return true;
		}

		foreach (var element in batch)
		{
			ProcessCreative(element);
			nextId = element.Id + 1;
		}

		return false;
	}
	
	protected abstract List<FbCreativeLoad> GetNextPendingCreativesBatch(int minimumId, int batchSize);
	
	protected abstract void ProcessCreative(FbCreativeLoad creative);
}