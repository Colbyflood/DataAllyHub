using DataAllyEngine.Common;
using DataAllyEngine.ContentProcessingTask;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;

namespace DataAllyEngine.Services.CreativeImagesLoader;

public class CreativeImagesLoadingService : AbstractCreativeLoader, ICreativeImagesLoadingService
{
	private readonly ILogger<ICreativeImagesLoadingService> logger;

	public CreativeImagesLoadingService(ILoaderProxy loaderProxy, ISchedulerProxy schedulerProxy, 
		ITokenHolder tokenHolder, ILogger<ICreativeImagesLoadingService> logger)
		: base(nameof(CreativeImagesLoadingService), loaderProxy, schedulerProxy, tokenHolder, logger)
	{
		this.logger = logger;
	}

	protected override List<FbCreativeLoad> GetNextPendingCreativesBatch(int minimumId, int batchSize)
	{
		return loaderProxy.GetPendingCreativeImages(minimumId, batchSize);
	}

	protected override void ProcessCreative(FbCreativeLoad creative)
	{
		throw new NotImplementedException();
	}
}