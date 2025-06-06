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

	protected override void ProcessCreative(FbCreativeLoad creative, TokenKey tokenKey, int channelId)
	{
		if (creative.BinId != null && !string.IsNullOrEmpty(creative.Guid))
		{
			return;
		}
		
		var now = DateTime.UtcNow;
		
		if (string.IsNullOrEmpty(creative.Url))
		{
			// attempt to decode the image from the hash
			var url = GetImageUrl(tokenKey, channelId, creative.CreativeKey);
			if (string.IsNullOrEmpty(url))
			{
				logger.LogWarning($"Cannot get creative image with key {creative.CreativeKey}.");
				creative.LastAttemptDateTimeUtc = now;
				creative.TotalAttempts++;
				loaderProxy.WriteFbCreativeLoad(creative);
				return;
			}
			loaderProxy.WriteFbCreativeLoad(creative);
			loaderProxy.WriteFbCreativeLoad(creative);
		}

		try
		{
			DownloadAndSaveCreative(creative);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Unable to download creative image for {creative.CreativeKey} (id {creative.Id}) because of {ex.Message}");
		}

		creative.LastAttemptDateTimeUtc = now;
		creative.TotalAttempts++;
		loaderProxy.WriteFbCreativeLoad(creative);
	}

	private string? GetImageUrl(TokenKey tokenKey,int channelId, string imageHash)
	{
		var channel = loaderProxy.GetChannelById(channelId);
		if (channel == null)
		{
			logger.LogWarning($"Channel with ID {channelId} not found");
			return null;
		}
		var facebookParameters = CreateFacebookParameters(tokenKey, channel);
		if (facebookParameters == null)
		{
			logger.LogWarning($"Facebook parameters for key {tokenKey} could not be created");
			return null;
		}

		try
		{
			var response = UrlIdDecoder.DecodeImageHash(facebookParameters, imageHash).GetAwaiter().GetResult();
			if (response.Count == 0)
			{
				logger.LogWarning($"No image found for hash {imageHash} for token {tokenKey} in Facebook");
				return null;
			}

			return response[0].Url;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Exception while decoding image hash {imageHash} for token {tokenKey}: {ex.Message}");
			return null;
		}
	}

	private void DownloadAndSaveCreative(FbCreativeLoad creative)
	{
		var filename = ExtractFilenameFromUrl(creative.Url!);
		var extension = ImageStorageTools.DeriveExtensionFromFilename(filename);
		
		var imageStream = ImageStorageTools.FetchFileToMemory(creative.Url!, extension);
		if (imageStream == null)
		{
			logger.LogWarning($"No image for creative {creative.CreativeKey}.");
		}

		var uuid = ImageStorageTools.GenerateGuid();
		var binId = ImageStorageTools.HashCreativeToBinId(uuid);
		
		SaveCreativeContentToBucket(creative, uuid, extension, binId, imageStream, filename);
	}
}