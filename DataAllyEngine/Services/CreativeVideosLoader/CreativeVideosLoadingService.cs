using DataAllyEngine.Common;
using DataAllyEngine.ContentProcessingTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;

namespace DataAllyEngine.Services.CreativeVideosLoader;

public class CreativeVideosLoadingService : ICreativeVideosLoadingService
{
	private const int ONE_MINUTE = 1;
	private const int ONE_MINUTE_MSEC = ONE_MINUTE * 60 * 1000;

	private const int FIVE_MINUTES = 5;
	private const int FIVE_MINUTES_MSEC = FIVE_MINUTES * 60 * 1000;

	private const int MINIMUM_WINDOW_MINUTES = 10;
	private const int MINIMUM_WINDOW_MSEC = MINIMUM_WINDOW_MINUTES * 60 * 1000;

	private const int MAXIMUM_WINDOW_HOURS = 10;
	private const int MAXIMUM_WINDOW_MSEC = MAXIMUM_WINDOW_HOURS * 60 * 60 * 1000;

	
	private const int PREEMPT_STUCK_MINUTES_BEFORE = 60;

	private const int MAXIMUM_DAYS_LOOKBACK = 2;
	private const int MAXIMUM_HOURS_LOOKBACK = MAXIMUM_DAYS_LOOKBACK * 24;

	private readonly IContentProcessor contentProcessor;
	private readonly ISchedulerProxy schedulerProxy;
	private readonly ILogger<ICreativeVideosLoadingService> logger;

	public CreativeVideosLoadingService(IContentProcessor contentProcessor, ISchedulerProxy schedulerProxy, ILogger<ICreativeVideosLoadingService> logger)
	{
		this.contentProcessor = contentProcessor;
		this.schedulerProxy = schedulerProxy;
		this.logger = logger;
	}
	
	public async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation("{ServiceName} working", nameof(CreativeVideosLoadingService));

		while (!stoppingToken.IsCancellationRequested)
		{
			CheckAndProcessPendingContent();
			await Task.Delay(FIVE_MINUTES_MSEC, stoppingToken);
		}
	}

	private void CheckAndProcessPendingContent()
	{
		var now = DateTime.UtcNow;

		// What to do here....???
	}
}