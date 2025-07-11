using DataAllyEngine.Common;
using DataAllyEngine.ContentProcessingTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using System.Collections.Concurrent;

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
    private const int IGNORE_START_MINUTES_BEFORE = 30;

    private const int MAXIMUM_DAYS_LOOKBACK = 2;
    private const int MAXIMUM_HOURS_LOOKBACK = MAXIMUM_DAYS_LOOKBACK * 24;

    private const int MAXIMUM_ATTEMPTS = 5;

    private readonly IContentProcessor contentProcessor;
    private readonly ISchedulerProxy schedulerProxy;
    private readonly ILoaderProxy loaderProxy;
    private readonly ILogger<IProcessContentService> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    public ProcessContentService(
          IContentProcessor contentProcessor
        , ISchedulerProxy schedulerProxy
        , ILoaderProxy loaderProxy
        , ILogger<IProcessContentService> logger
        , IServiceScopeFactory serviceScopeFactory
        )
    {
        this.contentProcessor = contentProcessor;
        this.schedulerProxy = schedulerProxy;
        this.loaderProxy = loaderProxy;
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        logger.LogInformation("{ServiceName} working", nameof(ProcessContentService));

        while (!stoppingToken.IsCancellationRequested)
        {
            CheckAndProcessPendingContent(stoppingToken);
            await Task.Delay(ONE_MINUTE_MSEC, stoppingToken);
        }
    }

    private SemaphoreSlim concurrencySemaphoreThreads = new SemaphoreSlim(10); // Limit to 10 concurrent tasks
    ConcurrentDictionary<int, (int, DateTime)> channelsCurrentProcessingDic = new ConcurrentDictionary<int, (int, DateTime)>();
    private void CheckAndProcessPendingContent(CancellationToken cancellationToken)
    {
        string warningMessage = $"CheckAndProcessPendingContent: Currently Semaphore Left: {concurrencySemaphoreThreads.CurrentCount} : {DateTime.Now}";
        string processingChannels = string.Join(", ", channelsCurrentProcessingDic.Keys);
        warningMessage += $" | Channels in processing: [{processingChannels}]";
        logger.LogWarning(warningMessage);

        var now = DateTime.UtcNow;
        var ignoreTimeWindow = now.AddMinutes(-1 * IGNORE_START_MINUTES_BEFORE); // 30 minutes
        var preemptTimeWindow = now.AddMinutes(-1 * PREEMPT_STUCK_MINUTES_BEFORE); // 1 hour
        var absoluteTimeWindow = now.AddHours(-1 * MAXIMUM_HOURS_LOOKBACK); // 1 day before

        var fbSaveContents = schedulerProxy.GetPendingFinishedFbRunLogsSaveContentsAfterDate(absoluteTimeWindow, MAXIMUM_ATTEMPTS);

        foreach (var fbSaveContent in fbSaveContents)
        {
            if (fbSaveContent.QueuedUtc == null)
            {
                InitiateQueProcessing(fbSaveContent.Id);
            }
            else if (fbSaveContent.QueuedUtc != null)
            {
                if (fbSaveContent.LastStartedUtc != null)
                {
                    ignoreTimeWindow = DateTime.UtcNow.AddMinutes(-1 * IGNORE_START_MINUTES_BEFORE); // 30 minutes

                    var lastStartedTime = DateTime.SpecifyKind(fbSaveContent.LastStartedUtc.Value, DateTimeKind.Utc);
                    if (lastStartedTime >= ignoreTimeWindow) // Ignore those which were already triggered in 30 minutes before window
                        continue;

                    if (fbSaveContent.HeartBeatLastReceivedAtUtc != null)
                    {
                        var heartBeatLastReceived = DateTime.SpecifyKind(fbSaveContent.HeartBeatLastReceivedAtUtc.Value, DateTimeKind.Utc);
                        if (heartBeatLastReceived >= ignoreTimeWindow)
                            continue;
                    }
                }

                if (concurrencySemaphoreThreads.CurrentCount < 1) // All threads are working
                {
                    logger.LogWarning($"Currently Semaphore Left: {concurrencySemaphoreThreads.CurrentCount}");
                    break;
                }

                int channelId = fbSaveContent.AdCreativeRunlog.ChannelId;
                int fbSaveContentId = fbSaveContent.Id;

                if (channelsCurrentProcessingDic.ContainsKey(channelId) && channelsCurrentProcessingDic.TryGetValue(channelId, out var channelsCurrentProcessingValue))
                {
                    var hoursInQueue = (DateTime.Now - channelsCurrentProcessingValue.Item2).TotalHours;

                    if (hoursInQueue > 6) // Something is stuck in it for more than a 6 hours 
                    {
                        logger.LogWarning($"ProcessContentService:CheckAndProcessPendingContent Channel stuck in queue. Channel:{channelId} fbSaveContentId:{fbSaveContentId} Hours:{hoursInQueue}");
                        //channelsCurrentProcessingDic.TryRemove(channelId, out _);
                    }
                    continue;
                }

                channelsCurrentProcessingDic.TryAdd(channelId, (fbSaveContentId, DateTime.Now));
                concurrencySemaphoreThreads.Wait(cancellationToken);

                var fbSaveContentLoaded = loaderProxy.GetFbSaveContentById(fbSaveContentId)!; // Ensure the content is loaded before processing
                fbSaveContentLoaded.LastStartedUtc = DateTime.UtcNow;
                fbSaveContentLoaded.Attempts += 1;
                schedulerProxy.WriteFbSaveContent(fbSaveContentLoaded);

#if DEBUG
                StartContentProcessingTask(serviceScopeFactory, fbSaveContentLoaded.Id).GetAwaiter().GetResult();

                channelsCurrentProcessingDic.TryRemove(channelId, out _);
                concurrencySemaphoreThreads.Release();
#else
                // Limit concurrency 
                var task = Task.Run(async () =>
                {
                    // Cashing values in local variables to avoid multiple property accesses from fbSaveContent
                    //int fbSaveContentId = fbSaveContent.Id;
                    //int channelid = fbSaveContent.AdCreativeRunlog.ChannelId;

                    try
                    {
                        await StartContentProcessingTask(serviceScopeFactory, fbSaveContentId);
                    }
                    finally
                    {
                        channelsCurrentProcessingDic.TryRemove(channelId, out _);
                        concurrencySemaphoreThreads.Release();
                    }
                }, cancellationToken);

#endif
            }
        }

    }

    private async Task StartContentProcessingTask(IServiceScopeFactory serviceScopeFactory, int fbSaveContentId)
    {
        logger.LogInformation($"Starting StartContentProcessingTask for fbSaveContentId {fbSaveContentId}");

        var contentProcessor = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IContentProcessor>();
        if (contentProcessor == null)
        {
            logger.LogError($"Unable to get contentProcessor for loading fbSaveContentId {fbSaveContentId}");
            return;
        }
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading fbSaveContentId {fbSaveContentId}");
            return;
        }
        var schedulerProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ISchedulerProxy>();
        if (schedulerProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading fbSaveContentId {fbSaveContentId}");
            return;
        }
        var processContentService = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILogger<IProcessContentService>>();
        if (processContentService == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading fbSaveContentId {fbSaveContentId}");
            return;
        }
        var iScopedService = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IServiceScopeFactory>();
        var service = new ProcessContentService(contentProcessor, schedulerProxy, loaderProxy, processContentService, iScopedService!);
        await service.CheckAndContinueProcessing(fbSaveContentId);
        logger.LogInformation($"Exiting started StartContentProcessingTask for fbSaveContentId {fbSaveContentId}");
    }

    [Obsolete]
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

        return response
            .OrderBy(x => x.SaveContent?.QueuedUtc ?? DateTime.MinValue)
            .ThenBy(x => x.SaveContent?.Sequence ?? int.MinValue)
            .ToList();
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

    private void InitiateQueProcessing(int fbSaveContentId)
    {
        var fbSaveContent = loaderProxy.GetFbSaveContentById(fbSaveContentId)!; // Ensure the content is loaded before processing

        fbSaveContent.QueuedUtc = DateTime.UtcNow;
        fbSaveContent.HeartBeatLastReceivedAtUtc = fbSaveContent.QueuedUtc;
        fbSaveContent.Attempts = 0;
        fbSaveContent.Sequence = 0;
        schedulerProxy.WriteFbSaveContent(fbSaveContent);
    }

    private void LaunchContentProcessing(FbSaveContent saveContent, int fbRunLogId)
    {
        FbRunLog runLog = loaderProxy.GetFbRunLogById(fbRunLogId)!;

        // processing must be sequential - creatives, then images, then insights
        var channel = schedulerProxy.GetChannelById(runLog.ChannelId);
        if (channel == null)
        {
            logger.LogError($"Unable to load channel {runLog.ChannelId} to initiate processing for FbSaveContent {saveContent.Id}");
            return;
        }

        contentProcessor.ProcessContentFor(channel, runLog, saveContent);
    }


    private async Task CheckAndContinueProcessing(int fbSaveContentId)
    {
        int sequenceProcessingRunning = -1;
        try
        {
            var fbSaveContent = schedulerProxy.LoadFbSaveContentById(fbSaveContentId)!;
            while (true)
            {
                switch (fbSaveContent.Sequence)
                {
                    case 0:
                        if (fbSaveContent.AdCreativeFinishedUtc == null)
                        {
                            sequenceProcessingRunning = 0;
                            LaunchContentProcessing(fbSaveContent, fbSaveContent.AdCreativeRunlogId!.Value);
                        }

                        fbSaveContent.Sequence = 1;
                        schedulerProxy.WriteFbSaveContent(fbSaveContent);

                        break;

                    case 1:
                        if (fbSaveContent.AdImageFinishedUtc == null)
                        {
                            sequenceProcessingRunning = 1;
                            LaunchContentProcessing(fbSaveContent, fbSaveContent.AdImageRunlogId!.Value);
                        }
                        fbSaveContent.Sequence = 2;
                        schedulerProxy.WriteFbSaveContent(fbSaveContent);

                        break;


                    case 2:
                        if (fbSaveContent.AdInsightFinishedUtc == null)
                        {
                            sequenceProcessingRunning = 2;
                            LaunchContentProcessing(fbSaveContent, fbSaveContent.AdInsightRunlogId!.Value);
                        }

                        fbSaveContent.Sequence = 3; // Mark as completed
                        schedulerProxy.WriteFbSaveContent(fbSaveContent);

                        return;

                    default:
                        logger.LogError($"Invalid sequence number {fbSaveContent.Sequence} for FbSaveContent {fbSaveContent.Id}");
                        return;
                }
            }
        }
        catch (Exception ex)
        {
            string exceptionMessage = $"{sequenceProcessingRunning} : {ex.Message} : {ex.InnerException?.Message} : {ex.StackTrace}";

            logger.LogError($"ProcessContentService:CheckAndContinueProcessing:  {exceptionMessage}");

            var fbContentLoaded = schedulerProxy.LoadFbSaveContentById(fbSaveContentId);
            if (fbContentLoaded != null)
            {
                fbContentLoaded.ErrorMessage = exceptionMessage;
                schedulerProxy.WriteFbSaveContent(fbContentLoaded);
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