using FacebookLoader.Common;
using FacebookLoader.Loader.AdCreative;
using FacebookLoader.Loader.AdImage;
using FacebookLoader.Loader.AdInsight;

namespace DataAllyEngine.LoaderTask;

public class LoaderTasks
{
	private readonly ILogger<LoaderTasks> logger;
	private readonly ILogging logging;
	
	public LoaderTasks(ILogger<LoaderTasks> logger)
	{
		this.logger = logger; 
		this.logging = new LoaderLogging(logger);
	}

	public void StartLoaderTask(FacebookParameters facebookParameters, string startDate, string endDate)
	{
		Task.Run(() => LoaderTask(facebookParameters, startDate, endDate, logging));
	}
	
	public void ResumeLoaderTask(FacebookParameters facebookParameters)
	{
		Task.Run(() => LoaderTask(facebookParameters, logging));
	}

	private static void AdCreativesLoaderTask(FacebookParameters facebookParameters, ILogging logger)
	{
		var adCreativesLoader = new AdCreativesLoader(facebookParameters, logger);
	}
	
	private static void AdImagesLoaderTask(FacebookParameters facebookParameters, ILogging logger)
	{
		var adImagesLoader = new AdImagesLoader(facebookParameters, logger);
	}
	
	private static void AdInsightsLoaderTask(FacebookParameters facebookParameters, string startDate, string endDate, ILogging logger)
	{
		var adInsightsLoader = new AdInsightsLoader(facebookParameters, logger);
	}
}