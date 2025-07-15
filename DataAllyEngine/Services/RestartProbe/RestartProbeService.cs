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

            var channel = schedulerProxy.GetChannelById(runlog.ChannelId, includeAccount: true);
            if (channel == null)
            {
                logger.LogError($"Could not find channel for candidate with channel Id {runlog.ChannelId}");
                continue;
            }

            if (channel?.Client?.Account?.Active == false)
            {
                logger.LogInformation($"Ignoring restart probe service for inactive account {channel!.Client!.AccountId} channel {channel!.Id}");
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

            var mostRecentStopProblem = GetMostRecentThrottleOrStallProblem(runlog);

            var restarted = false;
            var facebookParameters = new FacebookParameters(channel.ChannelAccountId, token.Token1);
            if (runlog.FeedType == Names.FEED_TYPE_AD_IMAGE)
            {
                if (mostRecentStopProblem == null || mostRecentStopProblem.RestartUrl == null)
                {
                    loaderRunner.StartAdImagesLoad(facebookParameters, runlog);
                }
                else
                {
                    loaderRunner.ResumeAdImagesLoad(facebookParameters, runlog, mostRecentStopProblem.RestartUrl);
                }
                restarted = true;
            }
            else if (runlog.FeedType == Names.FEED_TYPE_AD_CREATIVE)
            {
                if (mostRecentStopProblem == null || mostRecentStopProblem.RestartUrl == null)
                {
                    loaderRunner.StartAdCreativesLoad(facebookParameters, runlog);
                }
                else
                {
                    loaderRunner.ResumeAdCreativesLoad(facebookParameters, runlog, mostRecentStopProblem.RestartUrl);
                }
                restarted = true;
            }
            else if (runlog.FeedType == Names.FEED_TYPE_AD_INSIGHT)
            {
                if (mostRecentStopProblem == null || mostRecentStopProblem.RestartUrl == null)
                {
                    DateTime? startDate = null;
                    DateTime? endDate = null;

                    if (runlog.ScopeType == Names.SCOPE_TYPE_DAILY)
                    {
                        endDate = new DateTime(runlog.StartedUtc.Year, runlog.StartedUtc.Month, runlog.StartedUtc.Day, runlog.StartedUtc.Hour, 0, 0, DateTimeKind.Utc);
                        startDate = endDate.Value.AddDays(-7);
                    }
                    else // if (runlog.ScopeType == Names.SCOPE_TYPE_BACKFILL)
                    {
                        if (runlog.BackfillDays is not null)
                        {
                            endDate = new DateTime(runlog.StartedUtc.Year, runlog.StartedUtc.Month, runlog.StartedUtc.Day, runlog.StartedUtc.Hour, 0, 0, DateTimeKind.Utc);
                            startDate = endDate.Value.AddDays(-1 * runlog.BackfillDays.Value);
                        }
                    }

                    loaderRunner.StartAdInsightsLoad(facebookParameters, runlog, startDate, endDate);
                }
                else
                {
                    loaderRunner.ResumeAdInsightsLoad(facebookParameters, runlog, mostRecentStopProblem.RestartUrl);
                }
                restarted = true;
            }

            if (restarted && mostRecentStopProblem != null)
            {
                var uncachedRunProblem = schedulerProxy.GetFbRunProblemById(mostRecentStopProblem.Id);
                if (uncachedRunProblem != null)
                {
                    uncachedRunProblem.RestartedUtc = DateTime.UtcNow;
                    schedulerProxy.WriteFbRunProblem(uncachedRunProblem);
                }
            }

        }
        logger.LogInformation("Facebook probe event checker window complete");
    }

    private List<FbRunLog> FindStalledItems()
    {
        var now = DateTime.UtcNow;
        var ignoreTimeWindow = now.AddMinutes(-1 * IGNORE_START_MINUTES_BEFORE); // 30 minutes
        var preemptTimeWindow = now.AddMinutes(-1 * PREEMPT_STUCK_MINUTES_BEFORE); // 30 minutes
        var absoluteTimeWindow = now.AddHours(-1 * MAXIMUM_HOURS_LOOKBACK);

        // Fetch those runlogs which are not finished and started from absoluteTimeWindow, which is 24 hours ago
        var runlogs = schedulerProxy.GetUncachedIncompleteFbRunLogsSince(absoluteTimeWindow);
        var stalled = new List<FbRunLog>();
        foreach (var runlog in runlogs)
        {
            var lastStartedTime = DateTime.SpecifyKind(runlog.LastStartedUtc, DateTimeKind.Utc);
            if (lastStartedTime >= ignoreTimeWindow) // Ignore those which were already triggered in 30 minutes before window
                continue;

            var problem = GetCurrentProblem(runlog);
            if (problem == null)
            {
                if (lastStartedTime < preemptTimeWindow)
                {
                    logger.LogInformation($"Preempting possible stuck process and marking run runlog item {runlog.Id} as stalled ");

                    var uncachedRunlog = schedulerProxy.GetFbRunLogById(runlog.Id);
                    LogProblem(uncachedRunlog!, DateTime.UtcNow, Names.FB_PROBLEM_STALLED);

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

            if (problem.RestartAfterUtc != null)
            {
                var restartUtc = DateTime.SpecifyKind(problem.RestartAfterUtc.Value, DateTimeKind.Utc);
                if (restartUtc > now)
                {
                    logger.LogInformation($"Notice: {runlog.Id} is of reason {problem.Reason} and restart time not reached");
                    continue;
                }
            }

            if (problem.RestartedUtc != null)
            {
                var restartedUtc = DateTime.SpecifyKind(problem.RestartedUtc.Value, DateTimeKind.Utc);
                if (restartedUtc > preemptTimeWindow)
                {
                    logger.LogInformation($"Notice: {runlog.Id} is of reason {problem.Reason} and restarted happened after preempt");
                    continue;
                }
            }

            stalled.Add(runlog);
        }

        return stalled;
    }

    private FbRunProblem? GetCurrentProblem(FbRunLog runlog)
    {
        var problems = schedulerProxy.GetUncachedFbRunProblemsByRunlogIdOrderByDescendingCreated(runlog.Id);
        return problems.Count == 0 ? null : problems[0];
    }

    private FbRunProblem? GetMostRecentThrottleOrStallProblem(FbRunLog runlog)
    {
        var problems = schedulerProxy.GetUncachedFbRunProblemsByRunlogIdOrderByDescendingCreated(runlog.Id);
        return problems.FirstOrDefault(problem => problem.Reason == Names.FB_PROBLEM_THROTTLED || problem.Reason == Names.FB_PROBLEM_STALLED || problem.Reason == Names.FB_PROBLEM_TEMPORARY_DOWNTIME);
    }

    private void MarkTokenFailure(Channel channel, bool isTokenDisabled, FbRunLog runlog)
    {
        logger.LogWarning($"Token failure for channel Id {channel.Id} ({channel.ChannelAccountName})");

        var now = DateTime.UtcNow;
        var problem = isTokenDisabled ? Names.FB_PROBLEM_DISABLED_TOKEN : Names.FB_PROBLEM_BAD_TOKEN;

        var uncachedRunlog = schedulerProxy.GetFbRunLogById(runlog.Id);
        LogProblem(uncachedRunlog!, now, problem);
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