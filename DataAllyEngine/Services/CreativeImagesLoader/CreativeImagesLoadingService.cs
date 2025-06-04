using DataAllyEngine.Common;
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
		
		var now = DateTime.UtcNow;
		
		if (string.IsNullOrEmpty(creative.Url))
		{
			// attempt to decode the image from the hash
			var url = GetImageUrl();
			if (!string.IsNullOrEmpty(url))
			{
				logger.LogWarning($"Cannot get creative image with key {creative.CreativeKey}.");
				creative.LastAttemptDateTimedUtc = now;
				creative.TotalAttempts++;
				loaderProxy.WriteFbCreativeLoad(creative);
				return;
			}
		}

		try
		{
			DownloadAndSaveCreative(creative);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Unable to download creative image for {creative.CreativeKey} (id {creative.Id}) because of {ex.Message}");
		}

		creative.LastAttemptDateTimedUtc = now;
		creative.TotalAttempts++;
		loaderProxy.WriteFbCreativeLoad(creative);
	}

	private void DownloadAndSaveCreative(FbCreativeLoad creative)
	{
		var imageStream = ImageStorageTools.FetchFileToMemory(creative.Url!);
		if (imageStream == null)
		{
			Console.WriteLine($"[WARN] No image for creative {creative.CreativeKey}.");
		}

		var uuid = ImageStorageTools.GenerateGuid();
		var binId = ImageStorageTools.HashCreativeToBinId(uuid);
		var filename = ExtractFilenameFromUrl(creative.Url!);
		var extension = ImageStorageTools.DeriveExtensionFromFilename(filename);
		
		try
		{

			var s3Key = ImageStorageTools.AssembleS3Key(uuid, extension, binId);
			ImageStorageTools.SaveImageToS3(s3Client, imageStream!, creativesBucket, s3Key);
			
			creative.BinId = binId;
			creative.Guid = uuid;
			creative.Filename = filename;
			creative.Extension = extension;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[ERROR] Unable to save image for {filename} as {uuid} for {asset.AssetType} {asset.AssetKey}: {ex}");
		}
	}
}