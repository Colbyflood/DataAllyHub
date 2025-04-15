using DataAllyEngine.Common;
using DataAllyEngine.LoaderTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;

namespace DataAllyEngine.Services.BackfillLauncher;

public class BackfillLauncherService : IBackfillLauncherService
{
	// ReSharper disable InconsistentNaming
	private const int ONE_MINUTE = 1;
	private const int ONE_MINUTE_MSEC = ONE_MINUTE * 60 * 1000;
	private const string FACEBOOK_CHANNEL_NAME = "Facebook";
	
	private readonly ISchedulerProxy schedulerProxy;
	private readonly ILoaderRunner loaderRunner;
	private readonly ILogger<IBackfillLauncherService> logger;

	public BackfillLauncherService(ISchedulerProxy schedulerProxy, ILoaderRunner loaderRunner, ILogger<IBackfillLauncherService> logger)
	{
		this.schedulerProxy = schedulerProxy;
		this.loaderRunner = loaderRunner;
		this.logger = logger;
	}
	
	public async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation("{ServiceName} working", nameof(BackfillLauncherService));

		while (!stoppingToken.IsCancellationRequested)
		{
			RunPendingDailyServices();
			await Task.Delay(ONE_MINUTE_MSEC, stoppingToken);
		}
	}

	private void RunPendingDailyServices()
	{
		var channelType = schedulerProxy.GetChannelTypeByName(FACEBOOK_CHANNEL_NAME);
		if (channelType == null)
		{
			logger.LogError($"Fatal condition!  Could not find a channel type for '{FACEBOOK_CHANNEL_NAME}'");
			return;
		}
		
		var now = DateTime.UtcNow;
		var windowStartTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
			
		var candidates = schedulerProxy.GetBackfillRequests();

		foreach (var candidate in candidates)
		{
			var channel = schedulerProxy.GetChannelById(candidate.ChannelId);
			if (channel == null)
			{
				logger.LogError($"Could not find channel for candidate with channel Id {candidate.ChannelId}");
				continue;				
			}
			var company = schedulerProxy.GetCompanyByChannelId(candidate.ChannelId);
			if (company == null)
			{
				logger.LogError($"Could not find company for candidate with channel Id {candidate.ChannelId} ({channel.ChannelAccountName})");
				continue;
			}
			
			FbRunLog? adImageRunLog = null;
			FbRunLog? adCreativeRunLog = null;
			FbRunLog? adInsightRunLog = null;
			var runlogs = schedulerProxy.GetFbRunLogsByChannelIdAfterDate(candidate.ChannelId, Names.SCOPE_TYPE_BACKFILL, candidate.RequestedUtc);
			foreach (var runlog in runlogs)
			{
				if (runlog.FeedType == Names.FEED_TYPE_AD_IMAGE)
				{
					adImageRunLog = runlog;
				}
				else if (runlog.FeedType == Names.FEED_TYPE_AD_CREATIVE)
				{
					adCreativeRunLog = runlog;
				}
				else if (runlog.FeedType == Names.FEED_TYPE_AD_INSIGHT)
				{
					adInsightRunLog = runlog;
				}
			}

			if (adImageRunLog != null && adCreativeRunLog != null && adInsightRunLog != null)
			{
				schedulerProxy.DeleteFbBackfillRequest(candidate);
				continue;
			}
			
			var token = schedulerProxy.GetTokenByCompanyAndChannelType(company.Id, channelType);
			if (token == null)
			{
				logger.LogError($"Could not find token for candidate with channel Id {candidate.ChannelId} ({channel.ChannelAccountName})");
				MarkTokenFailure(channel, false, adImageRunLog, adCreativeRunLog, adInsightRunLog);
				continue;
			}

			if (token.Enabled == 0)
			{
				logger.LogError($"Token not enabled for candidate with channel Id {candidate.ChannelId} ({channel.ChannelAccountName})");
				MarkTokenFailure(channel, true, adImageRunLog, adCreativeRunLog, adInsightRunLog);
				continue;
			}
			
			
			logger.LogInformation($"Launching previously unstarted backfill loads for channel Id {candidate.ChannelId} ({channel.ChannelAccountName})");

			var facebookParameters = new FacebookParameters(channel.ChannelAccountId, token.Token1);
			if (adImageRunLog == null)
			{
				loaderRunner.StartAdImagesLoad(facebookParameters, channel, Names.SCOPE_TYPE_BACKFILL);
			}

			if (adCreativeRunLog == null)
			{
				loaderRunner.StartAdCreativesLoad(facebookParameters, channel, Names.SCOPE_TYPE_BACKFILL);
			}

			if (adInsightRunLog == null)
			{
				var startDate = now.AddDays(-1 * candidate.Days);
				loaderRunner.StartAdInsightsLoad(facebookParameters, channel, Names.SCOPE_TYPE_BACKFILL, startDate, now);
			}
		}
	}

	private void MarkTokenFailure(Channel channel, bool isTokenDisabled, FbRunLog? adImageRunLog, FbRunLog? adCreativeRunLog, FbRunLog? adInsightRunLog)
	{
		logger.LogWarning($"Token failure for channel Id {channel.Id} ({channel.ChannelAccountName})");
		
		var now = DateTime.UtcNow;
		var problem = isTokenDisabled ? Names.FB_PROBLEM_DISABLED_TOKEN : Names.FB_PROBLEM_BAD_TOKEN;


		if (adImageRunLog == null)
		{
			adImageRunLog = CreateRunLog(channel.Id, now, Names.FEED_TYPE_AD_IMAGE, Names.SCOPE_TYPE_DAILY);
		}
		LogProblem(adImageRunLog, now, problem);

		if (adCreativeRunLog == null)
		{
			adCreativeRunLog = CreateRunLog(channel.Id, now, Names.FEED_TYPE_AD_CREATIVE, Names.SCOPE_TYPE_DAILY);
		}
		LogProblem(adCreativeRunLog, now, problem);

		if (adInsightRunLog == null)
		{
			adInsightRunLog = CreateRunLog(channel.Id, now, Names.FEED_TYPE_AD_INSIGHT, Names.SCOPE_TYPE_DAILY);
		}
		LogProblem(adInsightRunLog, now, problem);
	}

	private FbRunLog CreateRunLog(int channelId, DateTime utcNow, string feedType, string scope)
	{
		var runlog = new FbRunLog();
		runlog.ChannelId = channelId;
		runlog.FeedType = feedType;
		runlog.StartedUtc = utcNow;
		runlog.ScopeType = scope;
		schedulerProxy.WriteFbRunLog(runlog);
		return runlog;
	}

	private void LogProblem(FbRunLog runLog, DateTime utcNow, string reason)
	{
		var runProblem = new FbRunProblem();
		runProblem.FbRunlogId = runLog.Id;
		runProblem.Reason = reason;
		runProblem.CreatedUtc = utcNow;
		schedulerProxy.WriteFbRunProblem(runProblem);
	}
}