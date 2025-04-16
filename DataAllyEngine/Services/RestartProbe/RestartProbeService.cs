using DataAllyEngine.Common;
using DataAllyEngine.LoaderTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.DailySchedule;
using FacebookLoader.Common;

namespace DataAllyEngine.Services.RestartProbe;

public class RestartProbeService : IRestartProbeService
{
	// ReSharper disable InconsistentNaming
	private const int TWO_MINUTES = 2;
	private const int TWO_MINUTES_MSEC = TWO_MINUTES * 60 * 1000;
	private const string FACEBOOK_CHANNEL_NAME = "Facebook";
	private const int RUNLOGS_PER_CANDIDATE = 3;

	private const int IGNORE_START_MINUTES_BEFORE = 30;

	private const int PREEMPT_STUCK_MINUTES_BEFORE = 30;

	private const int MAXIMUM_DAYS_LOOKBACK = 1;
	private const int MAXIMUM_HOURS_LOOKBACK = MAXIMUM_DAYS_LOOKBACK * 24;
	
	private readonly ISchedulerProxy schedulerProxy;
	private readonly ILoaderRunner loaderRunner;
	private readonly ILogger<IRestartProbeService> logger;

	public RestartProbeService(ISchedulerProxy schedulerProxy, ILoaderRunner loaderRunner, ILogger<IRestartProbeService> logger)
	{
		this.schedulerProxy = schedulerProxy;
		this.loaderRunner = loaderRunner;
		this.logger = logger;
	}
	
	public async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation("{ServiceName} working", nameof(RestartProbeService));

		while (!stoppingToken.IsCancellationRequested)
		{
			CheckAndRestartFailedJobs();
			await Task.Delay(TWO_MINUTES_MSEC, stoppingToken);
		}
	}

	private void CheckAndRestartFailedJobs()
	{
		logger.LogInformation("Facebook probe event checker window starting");
		var channelType = schedulerProxy.GetChannelTypeByName(FACEBOOK_CHANNEL_NAME);
		if (channelType == null)
		{
			logger.LogError($"Fatal condition!  Could not find a channel type for '{FACEBOOK_CHANNEL_NAME}'");
			return;
		}

		foreach (var runlog in FindStalledItems())
		{
			logger.LogInformation($"Restarting stalled item {runlog.Id} for channel {runlog.ChannelId} which is a {runlog.ScopeType} {runlog.FeedType}");
			
			var channel = schedulerProxy.GetChannelById(runlog.ChannelId);
			if (channel == null)
			{
				logger.LogError($"Could not find channel for candidate with channel Id {runlog.ChannelId}");
				continue;				
			}
			var company = schedulerProxy.GetCompanyByChannelId(runlog.ChannelId);
			if (company == null)
			{
				logger.LogError($"Could not find company for candidate with channel Id {runlog.ChannelId} ({channel.ChannelAccountName})");
				continue;
			}
			
			var token = schedulerProxy.GetTokenByCompanyAndChannelType(company.Id, channelType);
			if (token == null)
			{
				logger.LogError($"Could not find token for candidate with channel Id {runlog.ChannelId} ({channel.ChannelAccountName})");
				MarkTokenFailure(channel, false, runlog);
				continue;
			}

			if (token.Enabled == 0)
			{
				logger.LogError($"Token not enabled for candidate with channel Id {runlog.ChannelId} ({channel.ChannelAccountName})");
				MarkTokenFailure(channel, true, runlog);
				continue;
			}

			var mostRecentThrottle = GetMostRecentThrottleProblem(runlog);
			
			var facebookParameters = new FacebookParameters(channel.ChannelAccountId, token.Token1);
			if (runlog.FeedType == Names.FEED_TYPE_AD_IMAGE)
			{
				if (mostRecentThrottle == null || mostRecentThrottle.RestartUrl == null)
				{
					loaderRunner.StartAdImagesLoad(facebookParameters, runlog);
				}
				else
				{
					loaderRunner.ResumeAdImagesLoad(facebookParameters, runlog, mostRecentThrottle.RestartUrl);
				}
			}
			else if (runlog.FeedType == Names.FEED_TYPE_AD_CREATIVE)
			{
				if (mostRecentThrottle == null || mostRecentThrottle.RestartUrl == null)
				{
					loaderRunner.StartAdCreativesLoad(facebookParameters, runlog);
				}
				else
				{
					loaderRunner.ResumeAdCreativesLoad(facebookParameters, runlog, mostRecentThrottle.RestartUrl);
				}
			}
			else if (runlog.FeedType == Names.FEED_TYPE_AD_INSIGHT)
			{
				if (mostRecentThrottle == null || mostRecentThrottle.RestartUrl == null)
				{
					var endTime = new DateTime(runlog.StartedUtc.Year, runlog.StartedUtc.Month, runlog.StartedUtc.Day, runlog.StartedUtc.Hour, 0, 0, DateTimeKind.Utc);
					var startTime = endTime.AddDays(-1);

					loaderRunner.StartAdInsightsLoad(facebookParameters, runlog, startTime, endTime);
				}
				else
				{
					loaderRunner.ResumeAdInsightsLoad(facebookParameters, runlog, mostRecentThrottle.RestartUrl);
				}
			}
			
		}
		logger.LogInformation("Facebook probe event checker window complete");
	}
	
	private List<FbRunLog> FindStalledItems()
    {
	    var now = DateTime.UtcNow;
	    var ignoreTimeWindow = now.AddMinutes(-1 * IGNORE_START_MINUTES_BEFORE);
	    var preemptTimeWindow = now.AddMinutes(-1 * PREEMPT_STUCK_MINUTES_BEFORE);
	    var absoluteTimeWindow = now.AddHours(-1 * MAXIMUM_HOURS_LOOKBACK);

        var runlogs = schedulerProxy.GetIncompleteFbRunLogsSince(absoluteTimeWindow);
        var stalled = new List<FbRunLog>();
        foreach (var runlog in runlogs)
        { 
if (runlog.Id < 2694) continue;	        
            var startedTime = DateTime.SpecifyKind(runlog.StartedUtc, DateTimeKind.Utc);
            if (startedTime >= ignoreTimeWindow)
                continue;

            var problem = GetCurrentProblem(runlog);
            if (problem == null)
            {
                if (startedTime < preemptTimeWindow)
                {
	                logger.LogInformation($"Preempting possible stuck process and marking run runlog item {runlog.Id} as stalled ");
                    LogProblem(runlog, DateTime.UtcNow, Names.FB_PROBLEM_STALLED);
                    stalled.Add(runlog);
                }

                continue;
            }

            if (problem.Reason == Names.FB_PROBLEM_NO_TOKEN ||
                problem.Reason == Names.FB_PROBLEM_BAD_TOKEN ||
                
                problem.Reason == Names.FB_PROBLEM_NOT_PERMITTED)
            {
                continue;
            }

            if (problem.RestartAfterUtc == null)
            {
	            logger.LogInformation($"PROBLEM: {runlog.Id} is of reason {problem.Reason} and is not restart after utc");
                continue;
            }

            var restartUtc = DateTime.SpecifyKind(problem.RestartAfterUtc.Value, DateTimeKind.Utc);
            if (restartUtc > now)
            {
	            logger.LogInformation($"PROBLEM: {runlog.Id} is of reason {problem.Reason} and restart time not reached");
                continue;
            }

            if (problem.RestartedUtc.HasValue)
            {
                var restartedUtc = DateTime.SpecifyKind(problem.RestartedUtc.Value, DateTimeKind.Utc);
                if (restartedUtc > preemptTimeWindow)
                {
	                logger.LogInformation($"PROBLEM: {runlog.Id} is of reason {problem.Reason} and restarted after preempt");
                    continue;
                }
            }

            stalled.Add(runlog);
        }

        return stalled;
    }
	
	private FbRunProblem? GetCurrentProblem(FbRunLog runlog)
	{
		var problems = schedulerProxy.GetFbRunProblemsByRunlogIdOrderByDescendingCreated(runlog.Id);
		return problems.Count == 0 ? null : problems[0];
	}
	
	private FbRunProblem? GetMostRecentThrottleProblem(FbRunLog runlog)
	{
		var problems = schedulerProxy.GetFbRunProblemsByRunlogIdOrderByDescendingCreated(runlog.Id);
		return problems.FirstOrDefault(problem => problem.Reason == Names.FB_PROBLEM_THROTTLED);
	}
	
	private void MarkTokenFailure(Channel channel, bool isTokenDisabled, FbRunLog runlog)
	{
		logger.LogWarning($"Token failure for channel Id {channel.Id} ({channel.ChannelAccountName})");
		
		var now = DateTime.UtcNow;
		var problem = isTokenDisabled ? Names.FB_PROBLEM_DISABLED_TOKEN : Names.FB_PROBLEM_BAD_TOKEN;

		LogProblem(runlog, now, problem);
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