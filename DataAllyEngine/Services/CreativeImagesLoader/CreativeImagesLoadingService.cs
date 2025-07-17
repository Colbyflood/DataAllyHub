using Amazon.S3;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.CreativeLoader;
using FacebookLoader.Common;
using FacebookLoader.Content;
using FacebookLoader.UrlIdDecode;

namespace DataAllyEngine.Services.CreativeImagesLoader;

public class CreativeImagesLoadingService : AbstractCreativeLoader, ICreativeImagesLoadingService
{
    private new const int BATCH_SIZE = 1000;

    private readonly ILogger<ICreativeImagesLoadingService> logger;

    public CreativeImagesLoadingService(ILoaderProxy loaderProxy, ISchedulerProxy schedulerProxy,
        ITokenHolder tokenHolder, IConfigurationLoader configurationLoader,
        IAmazonS3 s3Client, ILogger<ICreativeImagesLoadingService> logger)
        : base(nameof(CreativeImagesLoadingService), loaderProxy, schedulerProxy, tokenHolder, configurationLoader, s3Client, logger)
    {
        this.logger = logger;
    }

    protected override List<FbCreativeLoad> GetNextPendingCreativesBatch(int minimumId, DateTime lastAttemptedIgnoreUtc)
    {
        return loaderProxy.GetPendingCreativeImages(minimumId, BATCH_SIZE, MAXIMUM_ATTEMPTS, lastAttemptedIgnoreUtc);
    }

    protected override void ProcessCreative(FbCreativeLoad creative, TokenKey tokenKey, int channelId)
    {
        if (creative.BinId != null && !string.IsNullOrEmpty(creative.Guid))
        {
            return;
        }

        string? errorMessage = null;

        try
        {
            if (string.IsNullOrEmpty(creative.Url))
            {
                // attempt to decode the image from the hash
                var imageUrlWidthHeight = GetImageUrlWidthHeight(tokenKey, channelId, creative.CreativeKey);
                if (string.IsNullOrEmpty(imageUrlWidthHeight?.Url))
                {
                    logger.LogWarning($"Cannot get creative image with key {creative.CreativeKey}.");
                    creative.LastAttemptDateTimeUtc = DateTime.UtcNow;
                    creative.TotalAttempts++;
                    loaderProxy.WriteFbCreativeLoad(creative);
                    return;
                }
                creative.Url = imageUrlWidthHeight?.Url;
                creative.Width = imageUrlWidthHeight?.Width;
                creative.Height = imageUrlWidthHeight?.Height;

                loaderProxy.WriteFbCreativeLoad(creative);
            }

            DownloadAndSaveCreative(creative);
        }
        catch (FacebookHttpException fe)
        {
            if (fe.Throttled)
            {
                logger.LogError(fe, $"Unable to download creative image for {creative.CreativeKey} (id {creative.Id}) because of throtling");
                Thread.Sleep(TimeSpan.FromMinutes(THIRTY_MINUTES)); // Wait for 30 minutes before retrying next to release throttling

                return;
            }
            else
            {
                errorMessage = $"Unable to download creative image for {creative.CreativeKey} (id {creative.Id}) because of feException: {fe.Message}";
                logger.LogError(fe, errorMessage);
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Unable to download creative image for {creative.CreativeKey} (id {creative.Id}) because of {ex.Message}";
            logger.LogError(ex, errorMessage);
        }

        creative.LastAttemptDateTimeUtc = DateTime.UtcNow;
        creative.TotalAttempts++;
        creative.ErrorMessage = errorMessage;
        loaderProxy.WriteFbCreativeLoad(creative);
    }

    private FacebookImageUrlWidthHeight GetImageUrlWidthHeight(TokenKey tokenKey, int channelId, string imageHash)
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

            return response[0]!;
        }
        catch (FacebookHttpException fe)
        {
            throw;
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