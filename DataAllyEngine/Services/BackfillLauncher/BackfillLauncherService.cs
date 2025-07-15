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
            RunPendingBackfillServices();
            await Task.Delay(ONE_MINUTE_MSEC, stoppingToken);
        }
    }

    private void RunPendingBackfillServices()
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
            var channel = schedulerProxy.GetChannelById(candidate.ChannelId, includeAccount: true);
            if (channel == null)
            {
                logger.LogError($"Could not find channel for candidate with channel Id {candidate.ChannelId}");
                continue;
            }

            if (channel?.Client?.Account?.Active == false)
            {
                logger.LogInformation($"Ignoring backfill launcher for inactive account {channel!.Client!.AccountId} channel {candidate.ChannelId}");
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
                MarkTokenFailure(channel, false, candidate.Days, adImageRunLog, adCreativeRunLog, adInsightRunLog);
                continue;
            }

            if (token.Enabled == 0)
            {
                logger.LogError($"Token not enabled for candidate with channel Id {candidate.ChannelId} ({channel.ChannelAccountName})");
                MarkTokenFailure(channel, true, candidate.Days, adImageRunLog, adCreativeRunLog, adInsightRunLog);
                continue;
            }

            logger.LogInformation($"Launching previously unstarted backfill loads for channel Id {candidate.ChannelId} ({channel.ChannelAccountName})");

            // Fetch existing saveContent
            var fbSaveContent = schedulerProxy.GetFbSaveContentByRunlogsIds(adCreativeRunLog?.Id, adImageRunLog?.Id, adInsightRunLog?.Id);
            if (fbSaveContent == null)
            {
                var saveContent = new FbSaveContent();
                saveContent.AdCreativeRunlogId = adCreativeRunLog?.Id;
                saveContent.AdImageRunlogId = adImageRunLog?.Id;
                saveContent.AdInsightRunlogId = adInsightRunLog?.Id;
                saveContent.StartedUtc = DateTime.UtcNow;
                saveContent.Attempts = 0;
                saveContent.Sequence = 0;
                schedulerProxy.WriteFbSaveContent(saveContent);

                fbSaveContent = saveContent;
            }

            var facebookParameters = new FacebookParameters(channel.ChannelAccountId, token.Token1);
            if (adImageRunLog == null)
            {
                loaderRunner.StartAdImagesLoad(facebookParameters, channel, Names.SCOPE_TYPE_BACKFILL, candidate.Days, fbSaveContent!.Id);
            }

            if (adCreativeRunLog == null)
            {
                loaderRunner.StartAdCreativesLoad(facebookParameters, channel, Names.SCOPE_TYPE_BACKFILL, candidate.Days, fbSaveContent!.Id);
            }

            if (adInsightRunLog == null)
            {
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (candidate.Days is not null)
                {
                    startDate = now.AddDays(-1 * candidate.Days.Value);
                    endDate = now.AddDays(-1);
                }

                loaderRunner.StartAdInsightsLoad(facebookParameters, channel, Names.SCOPE_TYPE_BACKFILL, startDate, endDate, candidate.Days, fbSaveContent!.Id);
            }

            Thread.Sleep(3000);// Sleep for 3 seconds to avoid overwhelming the API
            schedulerProxy.DeleteFbBackfillRequest(candidate); // remove candidate after running the backfill tasks

        }
    }

    private void MarkTokenFailure(Channel channel, bool isTokenDisabled, int? backfillDays, FbRunLog? adImageRunLog, FbRunLog? adCreativeRunLog, FbRunLog? adInsightRunLog)
    {
        logger.LogWarning($"Token failure for channel Id {channel.Id} ({channel.ChannelAccountName})");

        var now = DateTime.UtcNow;
        var problem = isTokenDisabled ? Names.FB_PROBLEM_DISABLED_TOKEN : Names.FB_PROBLEM_BAD_TOKEN;


        if (adImageRunLog == null)
        {
            adImageRunLog = CreateRunLog(channel.Id, now, Names.FEED_TYPE_AD_IMAGE, Names.SCOPE_TYPE_BACKFILL, backfillDays);
        }
        LogProblem(adImageRunLog, now, problem);

        if (adCreativeRunLog == null)
        {
            adCreativeRunLog = CreateRunLog(channel.Id, now, Names.FEED_TYPE_AD_CREATIVE, Names.SCOPE_TYPE_BACKFILL, backfillDays);
        }
        LogProblem(adCreativeRunLog, now, problem);

        if (adInsightRunLog == null)
        {
            adInsightRunLog = CreateRunLog(channel.Id, now, Names.FEED_TYPE_AD_INSIGHT, Names.SCOPE_TYPE_BACKFILL, backfillDays);
        }
        LogProblem(adInsightRunLog, now, problem);
    }

    private FbRunLog CreateRunLog(int channelId, DateTime utcNow, string feedType, string scope, int? backfillDays)
    {
        var runlog = new FbRunLog();
        runlog.ChannelId = channelId;
        runlog.FeedType = feedType;
        runlog.StartedUtc = utcNow;
        runlog.ScopeType = scope;
        runlog.LastStartedUtc = runlog.StartedUtc;
        runlog.BackfillDays = backfillDays;
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