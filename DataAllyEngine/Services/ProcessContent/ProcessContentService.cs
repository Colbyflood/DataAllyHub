using DataAllyEngine.Common;
using DataAllyEngine.ContentProcessingTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;

namespace DataAllyEngine.Services.ProcessContent;

public class ProcessContentService : IProcessContentService
{
	private const int ONE_MINUTE = 1;
	private const int ONE_MINUTE_MSEC = ONE_MINUTE * 60 * 1000;

	private const int MINIMUM_WINDOW_MINUTES = 10;
	private const int MINIMUM_WINDOW_MSEC = MINIMUM_WINDOW_MINUTES * 60 * 1000;

	private const int MAXIMUM_WINDOW_HOURS = 10;
	private const int MAXIMUM_WINDOW_MSEC = MAXIMUM_WINDOW_HOURS * 60 * 60 * 1000;

	
	private const int PREEMPT_STUCK_MINUTES_BEFORE = 60;

	private const int MAXIMUM_DAYS_LOOKBACK = 2;
	private const int MAXIMUM_HOURS_LOOKBACK = MAXIMUM_DAYS_LOOKBACK * 24;

	private readonly IContentProcessor contentProcessor;
	private readonly ISchedulerProxy schedulerProxy;
	private readonly ILogger<IProcessContentService> logger;

	public ProcessContentService(IContentProcessor contentProcessor, ISchedulerProxy schedulerProxy, ILogger<IProcessContentService> logger)
	{
		this.contentProcessor = contentProcessor;
		this.schedulerProxy = schedulerProxy;
		this.logger = logger;
	}
	
	public async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation("{ServiceName} working", nameof(ProcessContentService));

		while (!stoppingToken.IsCancellationRequested)
		{
			CheckAndProcessPendingContent();
			await Task.Delay(ONE_MINUTE_MSEC, stoppingToken);
		}
	}

	private void CheckAndProcessPendingContent()
	{
		var now = DateTime.UtcNow;
		var preemptTimeWindow = now.AddMinutes(-1 * PREEMPT_STUCK_MINUTES_BEFORE);
		var absoluteTimeWindow = now.AddHours(-1 * MAXIMUM_HOURS_LOOKBACK);

		foreach (var completeRunLog in FindCompletedUnprocessedRunLogs(absoluteTimeWindow))
		{
			if (completeRunLog.SaveContent == null)
			{
				InitiateProcessing(completeRunLog);
			}
			else if (completeRunLog.SaveContent.LastStartedUtc < preemptTimeWindow)
			{
				completeRunLog.SaveContent.LastStartedUtc = DateTime.UtcNow;
				completeRunLog.SaveContent.Attempts += 1;
				schedulerProxy.WriteFbSaveContent(completeRunLog.SaveContent);
				
				CheckAndContinueProcessing(completeRunLog);
			}
			else
			{
				logger.LogInformation($"Skipping processing of FbSaveContent {completeRunLog.SaveContent.Id} because it has not reached a preempt time.");
			}
		}
	}

	private List<RunLogsContainer> FindCompletedUnprocessedRunLogs(DateTime lookbackDateTime)
	{
		var response = new List<RunLogsContainer>();
		
		var runlogs = schedulerProxy.GetUncachedFinishedFbRunLogsAfterDate(lookbackDateTime);
		var adImageRunLogs = runlogs.Where(r => r.FeedType == Names.FEED_TYPE_AD_IMAGE).ToList();
		var adInsightRunLogs = runlogs.Where(r => r.FeedType == Names.FEED_TYPE_AD_INSIGHT).ToList();
		var adCreativeRunLogs = runlogs.Where(r => r.FeedType == Names.FEED_TYPE_AD_CREATIVE).ToList();

		foreach (var adImageRunLog in adImageRunLogs)
		{
			if (adImageRunLog.FinishedUtc == null)
			{
				continue;
			}
			var adInsightRunLog = FindMatchingRunLogIn(adImageRunLog, adInsightRunLogs, Names.FEED_TYPE_AD_INSIGHT);
			if (adInsightRunLog == null || adInsightRunLog.FinishedUtc == null)
			{
				continue;
			}
			var adCreativeRunLog = FindMatchingRunLogIn(adImageRunLog, adCreativeRunLogs, Names.FEED_TYPE_AD_CREATIVE);
			if (adCreativeRunLog == null || adCreativeRunLog.FinishedUtc == null)
			{
				continue;
			}
			var fbSaveContent = schedulerProxy.LoadFbSaveContentContainingRunlog(adImageRunLog.Id);
			if (!IsProcessingCompleteFor(fbSaveContent))
			{
				response.Add(new RunLogsContainer(adImageRunLog, adInsightRunLog, adCreativeRunLog, fbSaveContent));
			}
		}
		
		return response;
	}

	private FbRunLog? FindMatchingRunLogIn(FbRunLog toMatch, List<FbRunLog> runLogs, string feedType)
	{
		var match = FindMatchingRunLogInWithinTimeframe(toMatch, runLogs, MINIMUM_WINDOW_MSEC, feedType);
		if (match != null)
		{
			return match;
		}
		return FindMatchingRunLogInWithinTimeframe(toMatch, runLogs, MAXIMUM_WINDOW_MSEC, feedType);
	}
	
	private FbRunLog? FindMatchingRunLogInWithinTimeframe(FbRunLog toMatch, List<FbRunLog> runLogs, int msec, string feedType)
	{
		var after = toMatch.StartedUtc.AddMilliseconds(msec);
		var before = toMatch.StartedUtc.AddMilliseconds(-1 * msec);
		
		return runLogs.FirstOrDefault(r => r.ChannelId == toMatch.ChannelId && r.FeedType == feedType && r.StartedUtc >= before && r.StartedUtc <= after);
	}

	private bool IsProcessingCompleteFor(FbSaveContent? saveContent)
	{
		if (saveContent == null)
		{
			return false;
		}

		return saveContent.AdImageFinishedUtc != null && saveContent.AdInsightFinishedUtc != null && saveContent.AdCreativeFinishedUtc != null;
	}

	private void InitiateProcessing(RunLogsContainer container)
	{

		var saveContent = new FbSaveContent();
		saveContent.AdImageRunlogId = container.AdImagesRunLog.Id;
		saveContent.AdInsightRunlogId = container.AdInsightsRunLog.Id;
		saveContent.AdCreativeRunlogId = container.AdCreativesRunLog.Id;
		saveContent.QueuedUtc = DateTime.UtcNow;
		saveContent.StartedUtc = saveContent.QueuedUtc;
		saveContent.LastStartedUtc = saveContent.QueuedUtc;
		saveContent.Attempts = 1;
		saveContent.Sequence = 0;
		schedulerProxy.WriteFbSaveContent(saveContent);

		LaunchContentProcessing(container.AdCreativesRunLog, saveContent);
	}

	private void LaunchContentProcessing(FbRunLog runLog, FbSaveContent saveContent)
	{
		// processing must be sequential - creatives, then images, then insights
		var channel = schedulerProxy.GetChannelById(runLog.ChannelId);
		if (channel == null)
		{
			logger.LogError($"Unable to load channel {runLog.ChannelId} to initiate processing for FbSaveContent {saveContent.Id}");
			return;
		}

		contentProcessor.ProcessContentFor(channel, runLog);
	}

	private void CheckAndContinueProcessing(RunLogsContainer container)
	{
		while (true)
		{
			switch (container.SaveContent!.Sequence)
			{
				case 0:
					if (container.SaveContent.AdImageFinishedUtc == null)
					{
						LaunchContentProcessing(container.AdCreativesRunLog, container.SaveContent);
					}

					container.SaveContent.Sequence = 1;
					schedulerProxy.WriteFbSaveContent(container.SaveContent);

					break;

				case 1:
					if (container.SaveContent.AdImageFinishedUtc == null)
					{
						LaunchContentProcessing(container.AdImagesRunLog, container.SaveContent);
					}
					container.SaveContent.Sequence = 2;
					schedulerProxy.WriteFbSaveContent(container.SaveContent);

					break;


				case 2:
					if (container.SaveContent.AdCreativeFinishedUtc != null)
					{
						LaunchContentProcessing(container.AdInsightsRunLog, container.SaveContent);
					}
					return;

				default:
					logger.LogError($"Invalid sequence number {container.SaveContent.Sequence} for FbSaveContent {container.SaveContent.Id}");
					return;
			}
		}
	}
	
	
	
	class RunLogsContainer
	{
		public readonly FbRunLog AdImagesRunLog;
		public readonly FbRunLog AdInsightsRunLog;
		public readonly FbRunLog AdCreativesRunLog;
		public readonly FbSaveContent? SaveContent;

		public RunLogsContainer(FbRunLog adImagesRunLog, FbRunLog adInsightsRunLog, FbRunLog adCreativesRunLog, FbSaveContent? saveContent)
		{
			AdCreativesRunLog = adCreativesRunLog;
			AdImagesRunLog = adImagesRunLog;
			AdInsightsRunLog = adInsightsRunLog;
			SaveContent = saveContent;
		}
	}
}