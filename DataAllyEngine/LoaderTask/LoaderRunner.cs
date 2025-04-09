using DataAllyEngine.Common;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;
using FacebookLoader.Loader.AdCreative;
using FacebookLoader.Loader.AdImage;
using FacebookLoader.Loader.AdInsight;

namespace DataAllyEngine.LoaderTask;

public class LoaderRunner
{
	private readonly ILoaderProxy loaderProxy;
	private readonly ILogger<LoaderRunner> logger;
	private readonly ILogging logging;
	
	public LoaderRunner(ILoaderProxy loaderProxy, ILogger<LoaderRunner> logger)
	{
		this.loaderProxy = loaderProxy;
		this.logger = logger; 
		this.logging = new LoaderLogging(logger);
	}

	public void StartAdCreativesLoad(FacebookParameters facebookParameters, Channel channel, string scopeType)
	{
		Task.Run(() => StartAdCreativesLoadTask(facebookParameters, channel, scopeType, loaderProxy, logging));
	}

	public void ResumeAdCreativesLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url)
	{
		Task.Run(() => ResumeAdCreativesLoadTask(facebookParameters, runlog, url, loaderProxy, logging));
	}

	public void StartAdImagesLoad(FacebookParameters facebookParameters, Channel channel, string scopeType)
	{
		Task.Run(() => StartAdImagesLoadTask(facebookParameters, channel, scopeType, loaderProxy, logging));
	}

	public void ResumeAdImagesLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url)
	{
		Task.Run(() => ResumeAdImagesLoadTask(facebookParameters, runlog, url, loaderProxy, logging));
	}
	
	public void StartAdInsightsLoad(FacebookParameters facebookParameters, Channel channel, string scopeType, DateTime startDate, DateTime endDate)
	{
		Task.Run(() => StartAdInsightsLoadTask(facebookParameters, channel, scopeType, startDate, endDate, loaderProxy, logging));
	}

	public void ResumeAdInsightsLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url)
	{
		Task.Run(() => ResumeAdInsightsLoadTask(facebookParameters, runlog, url, loaderProxy, logging));
	}

	
	// Tasks to run in their own threads
	
	private static async Task StartAdCreativesLoadTask(FacebookParameters facebookParameters, Channel channel, string scopeType, ILoaderProxy loaderProxy, ILogging logger)
	{
		logger.LogInformation($"Starting AdCreativeLoadTask for channel {channel.Id}");
		var service = new FacebookAdCreativesService(channel, loaderProxy, facebookParameters, logger);
		await service.InitiateAdCreativesLoad(scopeType);
		logger.LogInformation($"Exiting started AdCreativeLoadTask for channel {channel.Id}");
	}

	private static async Task ResumeAdCreativesLoadTask(FacebookParameters facebookParameters, FbRunLog runlog, string url, ILoaderProxy loaderProxy, ILogging logger)
	{
		logger.LogInformation($"Resuming AdCreativeLoadTask for channel {runlog.ChannelId}");
		var channel = loaderProxy.GetChannelById(runlog.ChannelId);
		if (channel == null)
		{
			logger.LogError($"Resuming AdCreativeLoadTask for channel {runlog.ChannelId} failed because channel {runlog.ChannelId} not found");
			return;
		}
		var service = new FacebookAdCreativesService(channel, loaderProxy, facebookParameters, logger);
		await service.ResumeAdCreativesLoad(runlog, url);
		logger.LogInformation($"Exiting resumed AdCreativeLoadTask for channel {runlog.ChannelId}");
	}
	
	private static async Task StartAdImagesLoadTask(FacebookParameters facebookParameters, Channel channel, string scopeType, ILoaderProxy loaderProxy, ILogging logger)
	{
		logger.LogInformation($"Starting AdImageLoadTask for channel {channel.Id}");
		var service = new FacebookAdImagesService(channel, loaderProxy, facebookParameters, logger);
		await service.InitiateAdImagesLoad(scopeType);
		logger.LogInformation($"Exiting started AdImageLoadTask for channel {channel.Id}");
	}
	
	private static async Task ResumeAdImagesLoadTask(FacebookParameters facebookParameters, FbRunLog runlog, string url, ILoaderProxy loaderProxy, ILogging logger)
	{
		logger.LogInformation($"Resuming AdImageLoadTask for channel {runlog.ChannelId}");
		var channel = loaderProxy.GetChannelById(runlog.ChannelId);
		if (channel == null)
		{
			logger.LogError($"Resuming AdImageLoadTask for channel {runlog.ChannelId} failed because channel {runlog.ChannelId} not found");
			return;
		}
		var service = new FacebookAdImagesService(channel, loaderProxy, facebookParameters, logger);
		await service.ResumeAdImagesLoad(runlog, url);
		logger.LogInformation($"Exiting resumed AdImageLoadTask for channel {runlog.ChannelId}");
	}
	
	private static async Task StartAdInsightsLoadTask(FacebookParameters facebookParameters, Channel channel, string scopeType, 
		DateTime startDate, DateTime endDate, ILoaderProxy loaderProxy, ILogging logger)
	{
		logger.LogInformation($"Starting AdInsightLoadTask for channel {channel.Id} between {startDate} and {endDate}");
		var service = new FacebookAdInsightsService(channel, loaderProxy, facebookParameters, logger);
		await service.InitiateAdInsightsLoad(scopeType, startDate, endDate);
		logger.LogInformation($"Exiting started AdInsightLoadTask for channel {channel.Id}");
	}
	
	private static async Task ResumeAdInsightsLoadTask(FacebookParameters facebookParameters, FbRunLog runlog, string url, ILoaderProxy loaderProxy, ILogging logger)
	{
		logger.LogInformation($"Resuming AdInsightLoadTask for channel {runlog.ChannelId}");
		var channel = loaderProxy.GetChannelById(runlog.ChannelId);
		if (channel == null)
		{
			logger.LogError($"Resuming AdImageLoadTask for channel {runlog.ChannelId} failed because channel {runlog.ChannelId} not found");
			return;
		}
		var service = new FacebookAdInsightsService(channel, loaderProxy, facebookParameters, logger);
		await service.ResumeAdInsightsLoad(runlog, url);
		logger.LogInformation($"Exiting resumed AdInsightLoadTask for channel {runlog.ChannelId}");
	}
}