using Amazon.S3;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;
using FacebookLoader.Common;
using FacebookLoader.UrlIdDecode;

namespace DataAllyEngine.Services.CreativeVideosLoader;

public class CreativeVideosLoadingService : AbstractCreativeLoader, ICreativeVideosLoadingService
{

    private new const int BATCH_SIZE = 1000;

    private readonly ILogger<ICreativeVideosLoadingService> logger;

    public CreativeVideosLoadingService(ILoaderProxy loaderProxy, ISchedulerProxy schedulerProxy,
        ITokenHolder tokenHolder, IConfigurationLoader configurationLoader,
        IAmazonS3 s3Client, ILogger<ICreativeVideosLoadingService> logger)
        : base(nameof(CreativeVideosLoadingService), loaderProxy, schedulerProxy, tokenHolder, configurationLoader, s3Client, logger)
    {
        this.logger = logger;
    }

    protected override List<FbCreativeLoad> GetNextPendingCreativesBatch(int minimumId, DateTime lastAttemptedIgnoreUtc)
    {
        return loaderProxy.GetPendingCreativeVideos(minimumId, BATCH_SIZE, MAXIMUM_ATTEMPTS, lastAttemptedIgnoreUtc);
    }

    protected override void ProcessCreative(FbCreativeLoad creative, TokenKey tokenKey, int channelId)
    {
        if (creative.BinId != null && !string.IsNullOrEmpty(creative.Guid)) // Already processed
        {
            return;
        }

        string errorMessage = null;

        try
        {
            if (string.IsNullOrEmpty(creative.Url))
            {
                // attempt to decode the image from the hash
                var urlWithErrorMesage = GetVideoUrl(tokenKey, channelId, creative.CreativeKey);
                if (string.IsNullOrEmpty(urlWithErrorMesage.Item1)) // Url
                {
                    logger.LogWarning($"Cannot get creative video with key {creative.CreativeKey}.");
                    creative.LastAttemptDateTimeUtc = DateTime.UtcNow;
                    creative.TotalAttempts++;
                    creative.ErrorMessage = urlWithErrorMesage.Item2; // Error message
                    loaderProxy.WriteFbCreativeLoad(creative);
                    return;
                }

                creative.Url = urlWithErrorMesage.Item1; // Chance of improvements, To see if url signatures becomes an invalid, then get the new and use that

                loaderProxy.WriteFbCreativeLoad(creative);
            }

            DownloadAndSaveCreative(creative);
        }
        catch (FacebookHttpException fe)
        {
            if (fe.Throttled)
            {
                logger.LogError(fe, $"Unable to download creative video because facebook api for videos is throttled.");
                Thread.Sleep(TimeSpan.FromMinutes(THIRTY_MINUTES)); // Wait for 30 minutes before retrying next to release throttling

                return;
            }
            else
            {
                errorMessage = $"Unable to download creative video for {creative.CreativeKey} (id {creative.Id}) because of feException: {fe.Message}";
                logger.LogError(fe, errorMessage);
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Unable to download creative video for {creative.CreativeKey} (id {creative.Id}) because of {ex.Message}";
            logger.LogError(ex, errorMessage);
        }

        creative.LastAttemptDateTimeUtc = DateTime.UtcNow;
        creative.TotalAttempts++;
        creative.ErrorMessage = errorMessage;
        loaderProxy.WriteFbCreativeLoad(creative);
    }

    /// <summary>
    /// Returns url with error message in case of failure
    /// </summary>
    /// <param name="tokenKey"></param>
    /// <param name="channelId"></param>
    /// <param name="videoId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private (string?, string?) GetVideoUrl(TokenKey tokenKey, int channelId, string videoId)
    {
        string? errorMessage = null;

        var channel = loaderProxy.GetChannelById(channelId);
        if (channel == null)
        {
            errorMessage = $"Channel with ID {channelId} not found";
            logger.LogWarning(errorMessage);
            return (null, errorMessage);
        }

        var facebookParameters = CreateFacebookParameters(tokenKey, channel);
        if (facebookParameters == null)
        {
            errorMessage = $"Facebook parameters for key {tokenKey} could not be created";
            logger.LogWarning(errorMessage);
            return (null, errorMessage);
        }

        var pageToken = loaderProxy.GetFbAccountPageTokenById(tokenKey.CompanyId, tokenKey.PageId);
        if (string.IsNullOrWhiteSpace(pageToken?.PageAccessToken))
        {
            errorMessage = $"No page token found for key {tokenKey}";
            logger.LogWarning(errorMessage);
            return (null, errorMessage);
        }

        //var pageToken = tokenHolder.GetFacebookPageToken(tokenKey, channel).GetAwaiter().GetResult();
        //if (pageToken == null)
        //{
        //    logger.LogWarning($"No page token found for key {tokenKey}");
        //    return null;
        //}

        try
        {
            var response = UrlIdDecoder.DecodeVideoId(facebookParameters, pageToken.PageAccessToken, videoId).GetAwaiter().GetResult();
            if (response == null)
            {
                logger.LogWarning($"No video found for video id {videoId} for token {tokenKey} in Facebook");
                return (null, errorMessage);
            }

            return (response.Url, null);
        }
        catch (FacebookHttpException fe)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Exception while decoding video id {videoId}, ChannelId: {channelId} for token {tokenKey}: {ex.Message}");
            return (null, errorMessage);
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