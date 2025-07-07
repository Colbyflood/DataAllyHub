using Amazon.S3;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;
using FacebookLoader.UrlIdDecode;

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
			var url = GetVideoUrl(tokenKey, channelId, creative.CreativeKey);
			if (string.IsNullOrEmpty(url))
			{
				logger.LogWarning($"Cannot get creative video with key {creative.CreativeKey}.");
				creative.LastAttemptDateTimeUtc = now;
				creative.TotalAttempts++;
				loaderProxy.WriteFbCreativeLoad(creative);
				return;
			}
			creative.Url = url;

			loaderProxy.WriteFbCreativeLoad(creative);
		}

		try
		{
			DownloadAndSaveCreative(creative);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Unable to download creative video for {creative.CreativeKey} (id {creative.Id}) because of {ex.Message}");
		}

		creative.LastAttemptDateTimeUtc = now;
		creative.TotalAttempts++;
		loaderProxy.WriteFbCreativeLoad(creative);
	}

	private string? GetVideoUrl(TokenKey tokenKey, int channelId, string videoId)
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
		
		var pageToken = tokenHolder.GetFacebookPageToken(tokenKey, channel).GetAwaiter().GetResult();
		if (pageToken == null)
		{
			logger.LogWarning($"No page token found for key {tokenKey}");
			return null;
		}

		try
		{
			var response = UrlIdDecoder.DecodeVideoId(facebookParameters, pageToken.Token, videoId).GetAwaiter().GetResult();
			if (response == null)
			{
				logger.LogWarning($"No video found for video id {videoId} for token {tokenKey} in Facebook");
				return null;
			}

			return response.Url;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Exception while decoding video id {videoId} for token {tokenKey}: {ex.Message}");
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
			logger.LogWarning($"No video for creative {creative.CreativeKey}.");
		}

		var uuid = ImageStorageTools.GenerateGuid();
		var binId = ImageStorageTools.HashCreativeToBinId(uuid);

		
		SaveCreativeContentToBucket(creative, uuid, extension, binId, imageStream, filename);
	}
}