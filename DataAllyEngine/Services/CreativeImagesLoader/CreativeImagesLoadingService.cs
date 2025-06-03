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
		if (creative.BinId != null && !string.IsNullOrEmpty(creative.Guid))
		{
			return;
		}
		
		if (string.IsNullOrEmpty(creative.Url))
		{
			// attempt to decode the image from the hash
			var url = GetImageUrl();
			if (!string.IsNullOrEmpty(url))
			{
				logger.LogWarning($"Cannot get creative image with key {creative.CreativeKey}.");
				creative.LastAttemptDateTimedUtc = DateTime.UtcNow;
				loaderProxy.WriteFbCreativeLoad(creative);
				return;
			}
		}
		
		DownloadAndSave(creative);
	}
}