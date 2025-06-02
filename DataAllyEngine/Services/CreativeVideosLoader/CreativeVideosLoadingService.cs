using DataAllyEngine.Common;
using DataAllyEngine.ContentProcessingTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;

namespace DataAllyEngine.Services.CreativeVideosLoader;

public class CreativeVideosLoadingService : AbstractCreativeLoader, ICreativeVideosLoadingService
{
	private readonly ILogger<ICreativeVideosLoadingService> logger;

	public CreativeVideosLoadingService(ILoaderProxy loaderProxy, ISchedulerProxy schedulerProxy,
		ITokenHolder tokenHolder, ILogger<ICreativeVideosLoadingService> logger)
		: base(nameof(CreativeVideosLoadingService), loaderProxy, schedulerProxy, tokenHolder, logger)
	{
		this.logger = logger;
	}
	

}