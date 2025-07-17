using Amazon.S3;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;

namespace DataAllyEngine.Services.CreativeLoader;

public abstract class AbstractCreativeLoader
{
    // ReSharper disable InconsistentNaming
    protected const int ONE_MINUTE = 1;
    protected const int ONE_MINUTE_MSEC = ONE_MINUTE * 60 * 1000;

    protected const int FIVE_MINUTES = 5;
    protected const int FIVE_MINUTES_MSEC = FIVE_MINUTES * 60 * 1000;

    protected const int FIFTEEN_SEC = 15;
    protected const int FIFTEEN_SEC_MSEC = FIFTEEN_SEC * 1000;

    protected const int THIRTY_MINUTES = 30;
    protected const int ONE_HOUR_MINUTES = 60;
    protected const int RETRY_TIMEOUT_MINUTES = ONE_HOUR_MINUTES;
    protected const int RETRY_TIMEOUT_MSEC = RETRY_TIMEOUT_MINUTES * 60 * 1000; // 1 Hour milliseconds

    protected const int MAXIMUM_ATTEMPTS = 5;

    protected const int BATCH_SIZE = 200;

    protected readonly ILoaderProxy loaderProxy;
    protected readonly ISchedulerProxy schedulerProxy;
    protected readonly ITokenHolder tokenHolder;
    protected readonly IAmazonS3 s3Client;
    protected readonly string creativesBucket;

    private string serviceName;
    private ILogger logger;

    protected AbstractCreativeLoader(string serviceName, ILoaderProxy loaderProxy,
        ISchedulerProxy schedulerProxy, ITokenHolder tokenHolder, IConfigurationLoader configurationLoader,
        IAmazonS3 s3Client, ILogger logger)
    {
        this.serviceName = serviceName;
        this.loaderProxy = loaderProxy;
        this.schedulerProxy = schedulerProxy;
        this.tokenHolder = tokenHolder;
        creativesBucket = configurationLoader.GetKeyValueFor(Names.CREATIVES_BUCKET_KEY);
        this.s3Client = s3Client;
        this.logger = logger;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        logger.LogInformation("{ServiceName} working", serviceName);

        int startId = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            CheckAndProcessPendingContent(startId, out startId);

            await Task.Delay(FIVE_MINUTES_MSEC, stoppingToken);
        }
    }

    protected bool CheckAndProcessPendingContent(int startId, out int nextId)
    {
        var now = DateTime.UtcNow;
        var retryExpirationUtc = now.AddMilliseconds(RETRY_TIMEOUT_MSEC * -1);

        nextId = startId;

        var creativesBatch = GetNextPendingCreativesBatch(startId, retryExpirationUtc);

        if (creativesBatch.Count == 0)
        {
            return true;
        }

        foreach (var creative in creativesBatch)
        {
            ProcessCreative(creative, new TokenKey(creative.CompanyId, creative.CreativePageId), creative.ChannelId);
            nextId = creative.Id + 1;
        }

        return false;
    }

    protected abstract List<FbCreativeLoad> GetNextPendingCreativesBatch(int minimumId, DateTime lastAttemptedIgnoreUtc);

    protected abstract void ProcessCreative(FbCreativeLoad creative, TokenKey tokenKey, int channelId);

    protected static string ExtractFilenameFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }
        var path = url.Split("?")[0];
        return path.Split("/").Last();
    }

    protected FacebookParameters? CreateFacebookParameters(TokenKey key, Channel channel)
    {
        var token = loaderProxy.GetTokenByCompanyIdAndChannelTypeId(key.CompanyId, channel.ChannelTypeId);
        if (token == null)
        {
            logger.LogWarning($"Facebook token for company with ID {key.CompanyId} not found");
            return null;
        }

        return new FacebookParameters(channel.ChannelAccountId, token.Token1);
    }

    protected void SaveCreativeContentToBucket(FbCreativeLoad creative, string uuid, string extension, int binId, MemoryStream? imageStream, string filename)
    {
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
            logger.LogError($"Unable to save image for {filename} as {uuid} for creative {creative.Id} with image hash {creative.CreativeKey}: {ex}");
        }
    }
}