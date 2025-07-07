using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;


namespace DataAllyEngine.LoaderTask;

public class LoaderRunner : ILoaderRunner
{
    private readonly ILoaderProxy loaderProxy;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<LoaderRunner> logger;
    private readonly ILogging logging;

    public LoaderRunner(ILoaderProxy loaderProxy, IServiceScopeFactory serviceScopeFactory, ILogger<LoaderRunner> logger)
    {
        this.loaderProxy = loaderProxy;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
        this.logging = new LoaderLogging(logger);
    }

    public void StartAdCreativesLoad(FacebookParameters facebookParameters, Channel channel, string scopeType, int? backfillDays, int fbSaveContentId)
    {
#if DEBUG
        StartAdCreativesLoadTask(facebookParameters, channel, scopeType, backfillDays, serviceScopeFactory, logging, fbSaveContentId).GetAwaiter().GetResult();
#else
        Task.Run(() => StartAdCreativesLoadTask(facebookParameters, channel, scopeType, backfillDays, serviceScopeFactory, logging, fbSaveContentId));
#endif
    }

    public void StartAdCreativesLoad(FacebookParameters facebookParameters, FbRunLog runlog)
    {
        var channel = loaderProxy.GetChannelById(runlog.ChannelId);
        Task.Run(() => StartAdCreativesLoadTask(facebookParameters, runlog.Id, channel, serviceScopeFactory, logging));
    }

    public void ResumeAdCreativesLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url)
    {
        Task.Run(() => ResumeAdCreativesLoadTask(facebookParameters, runlog.Id, url, serviceScopeFactory, logging));
    }

    public void StartAdImagesLoad(FacebookParameters facebookParameters, Channel channel, string scopeType, int? backfillDays, int fbSaveContentId)
    {
#if DEBUG
        StartAdImagesLoadTask(facebookParameters, channel, scopeType, backfillDays, serviceScopeFactory, logging, fbSaveContentId).GetAwaiter().GetResult();
#else
        Task.Run(() => StartAdImagesLoadTask(facebookParameters, channel, scopeType, backfillDays, serviceScopeFactory, logging, fbSaveContentId));
#endif

    }

    public void StartAdImagesLoad(FacebookParameters facebookParameters, FbRunLog runlog)
    {
        var channel = loaderProxy.GetChannelById(runlog.ChannelId);
        Task.Run(() => StartAdImagesLoadTask(facebookParameters, runlog.Id, channel, serviceScopeFactory, logging));
    }

    public void ResumeAdImagesLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url)
    {
        Task.Run(() => ResumeAdImagesLoadTask(facebookParameters, runlog.Id, url, serviceScopeFactory, logging));
    }

    public void StartAdInsightsLoad(FacebookParameters facebookParameters, Channel channel, string scopeType, DateTime startDate, DateTime endDate, int? backfillDays, int fbSaveContentId)
    {

#if DEBUG
        StartAdInsightsLoadTask(facebookParameters, channel, scopeType, startDate, endDate, backfillDays, serviceScopeFactory, logging, fbSaveContentId).GetAwaiter().GetResult();
#else
        Task.Run(() => StartAdInsightsLoadTask(facebookParameters, channel, scopeType, startDate, endDate, backfillDays, serviceScopeFactory, logging, fbSaveContentId));
#endif
    }

    public void StartAdInsightsLoad(FacebookParameters facebookParameters, FbRunLog runlog, DateTime startDate, DateTime endDate)
    {
        var channel = loaderProxy.GetChannelById(runlog.ChannelId);
        Task.Run(() => StartAdInsightsLoadTask(facebookParameters, runlog.Id, channel, startDate, endDate, serviceScopeFactory, logging));
    }

    public void ResumeAdInsightsLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url)
    {
        Task.Run(() => ResumeAdInsightsLoadTask(facebookParameters, runlog.Id, url, serviceScopeFactory, logging));
    }


    // Tasks to run in their own threads

    private static async Task StartAdCreativesLoadTask(FacebookParameters facebookParameters, Channel channel, string scopeType, int? backfillDays, IServiceScopeFactory serviceScopeFactory, ILogging logger, int fbSaveContentId)
    {
        logger.LogInformation($"Starting AdCreativeLoadTask for channel {channel.Id}");
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading channel {channel.Id}");
            return;
        }
        var service = new FacebookAdCreativesService(channel, loaderProxy, facebookParameters, logger);
        await service.InitiateAdCreativesLoad(scopeType, backfillDays, fbSaveContentId);
        logger.LogInformation($"Exiting started AdCreativeLoadTask for channel {channel.Id}");
    }

    private static async Task StartAdCreativesLoadTask(FacebookParameters facebookParameters, int runlogId, Channel channel, IServiceScopeFactory serviceScopeFactory, ILogging logger)
    {
        logger.LogInformation($"Starting AdCreativeLoadTask for channel {channel.Id}");
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading channel {channel.Id}");
            return;
        }

        var runlog = loaderProxy.GetFbRunLogById(runlogId);
        if (runlog == null)
        {
            logger.LogError($"Impossible situation - Cannot read FbRunLog with id {runlogId} for loading channel {channel.Id}");
            return;
        }

        var service = new FacebookAdCreativesService(channel, loaderProxy, facebookParameters, logger);
        await service.StartAdCreativesLoad(runlog);
        logger.LogInformation($"Exiting started AdCreativeLoadTask for channel {runlog.ChannelId}");
    }

    private static async Task ResumeAdCreativesLoadTask(FacebookParameters facebookParameters, int runlogId, string url, IServiceScopeFactory serviceScopeFactory, ILogging logger)
    {
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for resuming AdCreativesLoadTask");
            return;
        }

        var runlog = loaderProxy.GetFbRunLogById(runlogId);
        if (runlog == null)
        {
            logger.LogError($"Impossible situation - Cannot read FbRunLog with id {runlogId} to resume AdCreativesLoadTask");
            return;
        }

        logger.LogInformation($"Resuming AdCreativesLoadTask for channel {runlog.ChannelId}");

        var channel = loaderProxy.GetChannelById(runlog.ChannelId);
        if (channel == null)
        {
            logger.LogError($"Resuming AdCreativesLoadTask for channel {runlog.ChannelId} failed because channel {runlog.ChannelId} not found");
            return;
        }
        var service = new FacebookAdCreativesService(channel, loaderProxy, facebookParameters, logger);
        await service.ResumeAdCreativesLoad(runlog, url);
        logger.LogInformation($"Exiting resumed AdCreativesLoadTask for channel {runlog.ChannelId}");
    }

    private static async Task StartAdImagesLoadTask(FacebookParameters facebookParameters, Channel channel, string scopeType, int? backfillDays, IServiceScopeFactory serviceScopeFactory, ILogging logger, int fbSaveContentId)
    {
        logger.LogInformation($"Starting AdImageLoadTask for channel {channel.Id}");
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading channel {channel.Id}");
            return;
        }
        var service = new FacebookAdImagesService(channel, loaderProxy, facebookParameters, logger);
        await service.InitiateAdImagesLoad(scopeType, backfillDays, fbSaveContentId);
        logger.LogInformation($"Exiting started AdImageLoadTask for channel {channel.Id}");
    }

    private static async Task StartAdImagesLoadTask(FacebookParameters facebookParameters, int runlogId, Channel channel, IServiceScopeFactory serviceScopeFactory, ILogging logger)
    {
        logger.LogInformation($"Starting AdImageLoadTask for channel {channel.Id}");
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading channel {channel.Id}");
            return;
        }

        var runlog = loaderProxy.GetFbRunLogById(runlogId);
        if (runlog == null)
        {
            logger.LogError($"Impossible situation - Cannot read FbRunLog with id {runlogId} for loading channel {channel.Id}");
            return;
        }
        var service = new FacebookAdImagesService(channel, loaderProxy, facebookParameters, logger);
        await service.StartAdImagesLoad(runlog);
        logger.LogInformation($"Exiting started AdImageLoadTask for channel {channel.Id}");
    }

    private static async Task ResumeAdImagesLoadTask(FacebookParameters facebookParameters, int runlogId, string url, IServiceScopeFactory serviceScopeFactory, ILogging logger)
    {
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for resuming AdImagesLoadTask");
            return;
        }

        var runlog = loaderProxy.GetFbRunLogById(runlogId);
        if (runlog == null)
        {
            logger.LogError($"Impossible situation - Cannot read FbRunLog with id {runlogId} to resume AdImagesLoadTask");
            return;
        }

        logger.LogInformation($"Resuming AdImagesLoadTask for channel {runlog.ChannelId}");

        var channel = loaderProxy.GetChannelById(runlog.ChannelId);
        if (channel == null)
        {
            logger.LogError($"Resuming AdImagesLoadTask for channel {runlog.ChannelId} failed because channel {runlog.ChannelId} not found");
            return;
        }
        var service = new FacebookAdImagesService(channel, loaderProxy, facebookParameters, logger);
        await service.ResumeAdImagesLoad(runlog, url);
        logger.LogInformation($"Exiting resumed AdImageLoadTask for channel {runlog.ChannelId}");
    }

    private static async Task StartAdInsightsLoadTask(FacebookParameters facebookParameters, int runlogId, Channel channel,
        DateTime startDate, DateTime endDate, IServiceScopeFactory serviceScopeFactory, ILogging logger)
    {
        logger.LogInformation($"Starting AdInsightLoadTask for channel {channel.Id} between {startDate} and {endDate}");
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading channel {channel.Id}");
            return;
        }

        var runlog = loaderProxy.GetFbRunLogById(runlogId);
        if (runlog == null)
        {
            logger.LogError($"Impossible situation - Cannot read FbRunLog with id {runlogId} for loading channel {channel.Id}");
            return;
        }
        var service = new FacebookAdInsightsService(channel, loaderProxy, facebookParameters, logger);
        await service.StartAdInsightsLoad(runlog, startDate, endDate);
        logger.LogInformation($"Exiting started AdInsightLoadTask for channel {channel.Id}");
    }

    private static async Task StartAdInsightsLoadTask(FacebookParameters facebookParameters, Channel channel, string scopeType,
        DateTime startDate, DateTime endDate, int? backfillDays, IServiceScopeFactory serviceScopeFactory, ILogging logger, int fbSaveContentId)
    {
        logger.LogInformation($"Starting AdInsightLoadTask for channel {channel.Id} between {startDate} and {endDate}");
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for loading channel {channel.Id}");
            return;
        }
        var service = new FacebookAdInsightsService(channel, loaderProxy, facebookParameters, logger);
        await service.InitiateAdInsightsLoad(scopeType, startDate, endDate, backfillDays, fbSaveContentId);
        logger.LogInformation($"Exiting started AdInsightLoadTask for channel {channel.Id}");
    }

    private static async Task ResumeAdInsightsLoadTask(FacebookParameters facebookParameters, int runlogId, string url,
        IServiceScopeFactory serviceScopeFactory, ILogging logger)
    {
        var loaderProxy = serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILoaderProxy>();
        if (loaderProxy == null)
        {
            logger.LogError($"Unable to get loaderProxy for resuming AdInsightsLoadTask");
            return;
        }

        var runlog = loaderProxy.GetFbRunLogById(runlogId);
        if (runlog == null)
        {
            logger.LogError($"Impossible situation - Cannot read FbRunLog with id {runlogId} to resume AdInsightsLoadTask");
            return;
        }

        logger.LogInformation($"Resuming AdInsightsLoadTask for channel {runlog.ChannelId}");

        var channel = loaderProxy.GetChannelById(runlog.ChannelId);
        if (channel == null)
        {
            logger.LogError($"Resuming AdInsightsLoadTask for channel {runlog.ChannelId} failed because channel {runlog.ChannelId} not found");
            return;
        }
        var service = new FacebookAdInsightsService(channel, loaderProxy, facebookParameters, logger);
        await service.ResumeAdInsightsLoad(runlog, url);
        logger.LogInformation($"Exiting resumed AdInsightsLoadTask for channel {runlog.ChannelId}");
    }
}