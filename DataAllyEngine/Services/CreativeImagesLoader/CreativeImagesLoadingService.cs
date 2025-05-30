using DataAllyEngine.Common;
using DataAllyEngine.ContentProcessingTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;

namespace DataAllyEngine.Services.CreativeImagesLoader;

public class CreativeImagesLoadingService : AbstractCreativeLoader, ICreativeImagesLoadingService
{

	private readonly IContentProcessor contentProcessor;
	private readonly ISchedulerProxy schedulerProxy;
	private readonly ILogger<ICreativeImagesLoadingService> logger;

	public CreativeImagesLoadingService(IContentProcessor contentProcessor, ISchedulerProxy schedulerProxy, ILogger<ICreativeImagesLoadingService> logger)
		: base(nameof(CreativeImagesLoadingService), logger)
	{
		this.contentProcessor = contentProcessor;
		this.schedulerProxy = schedulerProxy;
		this.logger = logger;
	}
}