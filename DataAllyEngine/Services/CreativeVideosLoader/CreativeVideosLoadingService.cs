using Amazon.S3;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;

namespace DataAllyEngine.Services.CreativeVideosLoader;

public class CreativeVideosLoadingService : AbstractCreativeLoader, ICreativeVideosLoadingService
{
	private readonly ILogger<ICreativeVideosLoadingService> logger;

	public CreativeVideosLoadingService(ILoaderProxy loaderProxy, ISchedulerProxy schedulerProxy,
		ITokenHolder tokenHolder, IConfigurationLoader configurationLoader,
		IAmazonS3 s3Client, ILogger<ICreativeVideosLoadingService> logger)
		: base(nameof(CreativeVideosLoadingService), loaderProxy, schedulerProxy, tokenHolder, configurationLoader, s3Client, logger)
	{
		this.logger = logger;
	}


	protected override List<FbCreativeLoad> GetNextPendingCreativesBatch(int minimumId, int batchSize)
	{
		return loaderProxy.GetPendingCreativeVideos(minimumId, batchSize);
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
			var url = GetVideoUrl();
			if (!string.IsNullOrEmpty(url))
			{
				logger.LogWarning($"Cannot get creative video with key {creative.CreativeKey}.");
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
			logger.LogError(ex, $"Unable to download creative video for {creative.CreativeKey} (id {creative.Id}) because of {ex.Message}");
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
			Console.WriteLine($"[WARN] No video for creative {creative.CreativeKey}.");
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
			Console.WriteLine($"[ERROR] Unable to save video for {filename} as {uuid} for {asset.AssetType} {asset.AssetKey}: {ex}");
		}
	}
}