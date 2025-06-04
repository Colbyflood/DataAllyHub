using Amazon.S3;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;
using FacebookLoader.UrlIdDecode;

namespace DataAllyEngine.Services.CreativeImagesLoader;

public class CreativeImagesLoadingService : AbstractCreativeLoader, ICreativeImagesLoadingService
{
	private readonly ILogger<ICreativeImagesLoadingService> logger;

	public CreativeImagesLoadingService(ILoaderProxy loaderProxy, ISchedulerProxy schedulerProxy, 
		ITokenHolder tokenHolder, IConfigurationLoader configurationLoader,
		IAmazonS3 s3Client, ILogger<ICreativeImagesLoadingService> logger)
		: base(nameof(CreativeImagesLoadingService), loaderProxy, schedulerProxy, tokenHolder, configurationLoader, s3Client, logger)
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
		var tokenKey = new TokenKey(creative.CompanyId, creative.ChannelId);
		
		if (string.IsNullOrEmpty(creative.Url))
		{
			// attempt to decode the image from the hash
			var url = GetImageUrl(tokenKey, creative.CreativeKey);
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

	private string? GetImageUrl(TokenKey tokenKey, string imageHash)
	{
		var facebookParameters = CreateFacebookParameters(tokenKey);
		if (facebookParameters == null)
		{
			logger.LogWarning($"Facebook parameters for key {tokenKey} could not be created");
			return null;
		}
		
		var response = UrlIdDecoder.DecodeImageHash(facebookParameters, imageHash).GetAwaiter().GetResult();
		if (response.Count == 0)
		{
			logger.LogWarning($"No image found for hash {imageHash} for token {tokenKey} in Facebook");
			return null;
		}

		return response[0].Url;
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