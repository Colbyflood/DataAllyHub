using DataAllyEngine.Common;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;
using FacebookLoader.Loader.AdCreative;
using FacebookLoader.Loader.AdImage;
using FacebookLoader.Loader.AdInsight;

namespace DataAllyEngine.LoaderTask;

public class LoaderTasks
{
	private readonly ILoaderProxy loaderProxy;
	private readonly FacebookParameters facebookParameters;
	private readonly ILogger<LoaderTasks> logger;
	private readonly ILogging logging;
	
	public LoaderTasks(ILoaderProxy loaderProxy, FacebookParameters facebookParameters, ILogger<LoaderTasks> logger)
	{
		this.loaderProxy = loaderProxy;
		this.facebookParameters = facebookParameters;
		this.logger = logger; 
		this.logging = new LoaderLogging(logger);
	}

	public void StartLoaderTask(string startDate, string endDate)
	{
		Task.Run(() => LoaderTask(facebookParameters, startDate, endDate, logging));
	}
	
	public void ResumeLoaderTask()
	{
		Task.Run(() => LoaderTask(facebookParameters, logging));
	}

	private static void AdCreativesLoaderTask(ILoaderProxy loaderProxy, FacebookParameters facebookParameters, ILogging logger)
	{
		logger.LogInformation($"Requesting loading of ad creatives for channel {channelId} in scope {scopeType}");

		var runlog = new FbRunLog();
		runlog.ChannelId = channelId;
		runlog.FeedType = Names.FEED_TYPE_AD_CREATIVE;
		runlog.ScopeType = scopeType;
		runlog.StartedUtc = DateTime.UtcNow;
		loaderProxy.WriteFbRunLog(runlog);

		var adCreativesLoader = new AdCreativesLoader(facebookParameters, logger);
		success = self.start_ad_creative_load(runlog)
			
		if success:
			runlog.FinishedUtc = DateTime.UtcNow;
			loaderProxy.WriteFbRunLog(runlog);
		return runlog.get_id()
		return None
			
			
	}
	
	private static void AdImagesLoaderTask(ILoaderProxy loaderProxy, FacebookParameters facebookParameters, ILogging logger)
	{
		var adImagesLoader = new AdImagesLoader(facebookParameters, logger);
	}
	
	private static void AdInsightsLoaderTask(ILoaderProxy loaderProxy, FacebookParameters facebookParameters, string startDate, string endDate, ILogging logger)
	{
		var adInsightsLoader = new AdInsightsLoader(facebookParameters, logger);
	}
}