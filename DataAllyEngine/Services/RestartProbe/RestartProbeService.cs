using DataAllyEngine.Common;
using DataAllyEngine.LoaderTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.DailySchedule;

namespace DataAllyEngine.Services.RestartProbe;

public class RestartProbeService : IRestartProbeService
{
	// ReSharper disable InconsistentNaming
	private const int FIVE_MINUTES = 5;
	private const int FIVE_MINUTES_MSEC = FIVE_MINUTES * 60 * 1000;
	private const string FACEBOOK_CHANNEL_NAME = "Facebook";
	private const int RUNLOGS_PER_CANDIDATE = 3;

	private const int IGNORE_START_MINUTES_BEFORE = 30;

	private const int PREEMPT_STUCK_MINUTES_BEFORE = 30;

	private const int MAXIMUM_DAYS_LOOKBACK = 7;
	private const int MAXIMUM_HOURS_LOOKBACK = MAXIMUM_DAYS_LOOKBACK * 24;
	
	private readonly ISchedulerProxy schedulerProxy;
	private readonly ILoaderRunner loaderRunner;
	private readonly ILogger<RestartProbeService> logger;

	public RestartProbeService(ISchedulerProxy schedulerProxy, ILoaderRunner loaderRunner, ILogger<RestartProbeService> logger)
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
			await Task.Delay(FIVE_MINUTES_MSEC, stoppingToken);
		}
	}

	private void CheckAndRestartFailedJobs()
	{
		logger.LogInformation("Facebook probe event checker starting");
		var channelType = schedulerProxy.GetChannelTypeByName(FACEBOOK_CHANNEL_NAME);
		if (channelType == null)
		{
			logger.LogError($"Fatal condition!  Could not find a channel type for '{FACEBOOK_CHANNEL_NAME}'");
			return;
		}

		foreach (var runlog in FindStalledItems())
		{
			logger.LogInformation($"Restarting stalled item {runlog.Id} for channel {runlog.ChannelId} which is a {runlog.ScopeType} {runlog.FeedType}");
		}


	}
	
	private List<FbRunLog> FindStalledItems()
    {
	    var now = DateTime.UtcNow;
	    var ignoreTimeWindow = now.AddMinutes(-1 * IGNORE_START_MINUTES_BEFORE);
	    var preemptTimeWindow = now.AddMinutes(-1 * PREEMPT_STUCK_MINUTES_BEFORE);
	    var absoluteTimeWindow = now.AddHours(-1 * MAXIMUM_HOURS_LOOKBACK);
        var stalled = new List<FbRunLog>();

        foreach (var runlog in schedulerProxy.GetIncompleteFbRunLogsSince(absoluteTimeWindow))
        {
            var startedTime = DateTime.SpecifyKind(runlog.StartedUtc, DateTimeKind.Utc);
            if (startedTime >= ignoreTimeWindow)
                continue;

            var problem = GetCurrentProblem(runlog);
            if (problem == null)
            {
                logger.LogInformation($"PROBLEM: {runlog.Id} not found");

                if (startedTime < preemptTimeWindow)
                {
	                logger.LogInformation($"Preempting possible stuck process and marking run runlog item {runlog.Id} as stalled ");
                    LogProblem(runlog, DateTime.UtcNow, Names.FB_PROBLEM_STALLED);
                    stalled.Add(runlog);
                }

                continue;
            }

            logger.LogInformation($"PROBLEM: {runlog.Id} is of reason {problem.Reason}");

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

            logger.LogInformation($"PROBLEM: {runlog.Id} is of reason {problem.Reason} GOT ONE");
            stalled.Add(runlog);
        }

        return stalled;
    }
	
	private FbRunProblem? GetCurrentProblem(FbRunLog runlog)
	{
		var problems = schedulerProxy.GetFbRunProblemsByRunlogIdOrderByDescendingCreated(runlog.Id);
		return problems.Count == 0 ? null : problems[0];
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