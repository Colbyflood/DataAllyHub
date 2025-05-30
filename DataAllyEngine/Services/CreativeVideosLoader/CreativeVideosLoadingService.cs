using DataAllyEngine.Common;
using DataAllyEngine.ContentProcessingTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;

namespace DataAllyEngine.Services.CreativeVideosLoader;

public class CreativeVideosLoadingService : AbstractCreativeLoader, ICreativeVideosLoadingService
{
	private readonly IContentProcessor contentProcessor;
	private readonly ISchedulerProxy schedulerProxy;
	private readonly ILogger<ICreativeVideosLoadingService> logger;

	public CreativeVideosLoadingService(IContentProcessor contentProcessor, ISchedulerProxy schedulerProxy, ILogger<ICreativeVideosLoadingService> logger)
		: base(nameof(CreativeVideosLoadingService), logger)
	{
		this.contentProcessor = contentProcessor;
		this.schedulerProxy = schedulerProxy;
		this.logger = logger;
	}
	

}