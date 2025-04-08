using FacebookLoader.Common;
using FacebookLoader.Loader.AdInsight;

namespace DataAllyEngine.LoaderTask;

public class AdCreativeLoaderTask
{
	private readonly ILogger<AdCreativeLoaderTask> logger;
	
	public AdCreativeLoaderTask(ILogger<AdCreativeLoaderTask> logger)
	{
		this.logger = logger;
	}

	public void StartLoaderTask(FacebookParameters facebookParameters, string startDate, string endDate)
	{
		Task.Run(() => LoaderTask(facebookParameters, startDate, endDate, new LoaderLogging(logger)));
	}
	
	public void ResumeLoaderTask(FacebookParameters facebookParameters)
	{
		Task.Run(() => LoaderTask(facebookParameters, new LoaderLogging(logger)));
	}

	private static void LoaderTask(FacebookParameters facebookParameters, string startDate, string endDate, ILogging logger)
	{
		var adInsightsLoader = new AdInsightsLoader(facebookParameters, logger);
	}
}