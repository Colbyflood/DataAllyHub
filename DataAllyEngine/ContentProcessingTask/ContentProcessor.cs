namespace DataAllyEngine.ContentProcessingTask;

public class ContentProcessor
{
	 private readonly Channel _channel;
    private readonly string _thumbnailBucket;
    private readonly DbConnection _dbConnection;

    private readonly FbRunStagingProxy _fbRunStagingProxy;
    private readonly CampaignProxy _campaignProxy;
    private readonly AdSetProxy _adSetProxy;
    private readonly AdProxy _adProxy;
    private readonly AssetProxy _assetProxy;
    private readonly AdMetadataProxy _adMetadataProxy;
    private readonly AdCopyProxy _adCopyProxy;
    private readonly ThumbnailProxy _thumbnailProxy;

    public ContentProcessor(Channel channel, string thumbnailBucket, DbConnection dbConnection)
    {
        _channel = channel;
        _thumbnailBucket = thumbnailBucket;
        _dbConnection = dbConnection;

        _fbRunStagingProxy = new FbRunStagingProxy(dbConnection);
        _campaignProxy = new CampaignProxy(dbConnection);
        _adSetProxy = new AdSetProxy(dbConnection);
        _adProxy = new AdProxy(dbConnection);
        _assetProxy = new AssetProxy(dbConnection);
        _adMetadataProxy = new AdMetadataProxy(dbConnection);
        _adCopyProxy = new AdCopyProxy(dbConnection);
        _thumbnailProxy = new ThumbnailProxy(dbConnection);
    }

    public void Process(FbRunLog runlog)
    {
        if (runlog.FeedType == FbRunLog.FEED_TYPE_AD_INSIGHT)
        {
            ProcessAdInsights(runlog);
        }
        else if (runlog.FeedType == FbRunLog.FEED_TYPE_AD_CREATIVE)
        {
            ProcessAdCreatives(runlog);
        }
        else
        {
            ProcessAdImages(runlog);
        }
    }

    private void ProcessAdCreatives(FbRunLog runlog)
    {
        var stagingEntries = _fbRunStagingProxy.GetByRunlog(runlog.Id);
        foreach (var entry in stagingEntries)
        {
            Console.WriteLine("Deserializing a staging entry for ad creatives");
            foreach (var record in FacebookAdCreativeTools.Deserialize(entry.Content))
            {
                var adsetId = PrepareAdHierarchy(record.AdSetId, $"AdSet {record.AdSetId}",
                                                 record.CampaignId, $"Campaign {record.CampaignId}",
                                                 0, record.CreatedTime, _channel);
                PrepareAd(adsetId, record);
            }
        }
        CleanStaging(stagingEntries);
    }

    private void ProcessAdImages(FbRunLog runlog)
    {
        var stagingEntries = _fbRunStagingProxy.GetByRunlog(runlog.Id);
        Console.WriteLine("Deserializing a staging entry for ad images");
        foreach (var entry in stagingEntries)
        {
            foreach (var record in FacebookAdImageTools.Deserialize(entry.Content))
            {
                PrepareImages(record);
            }
        }
        CleanStaging(stagingEntries);
    }

    private void ProcessAdInsights(FbRunLog runlog)
    {
        var stagingEntries = _fbRunStagingProxy.GetByRunlog(runlog.Id);
        if (stagingEntries == null || !stagingEntries.Any()) return;

        var kpiProcessor = new KpiProcessor(_channel, _dbConnection);
        foreach (var entry in stagingEntries)
        {
            Console.WriteLine("Deserializing a staging entry for ad insights");
            foreach (var record in FacebookAdInsightTools.Deserialize(entry.Content))
            {
                PrepareKpis(kpiProcessor, record);
            }
        }
        CleanStaging(stagingEntries);
    }

    private void CleanStaging(List<FbRunStaging> stagingEntries)
    {
        foreach (var entry in stagingEntries)
        {
            _fbRunStagingProxy.Delete(entry);
        }
    }

    public static string NormalizeNameFor(string name)
    {
        var workingName = name.ToUpper();
        while (true)
        {
            if (workingName.EndsWith(" - COPY")) workingName = workingName[..^7];
            else if (workingName.EndsWith("- COPY")) workingName = workingName[..^6];
            else if (workingName.EndsWith(" -COPY")) workingName = workingName[..^6];
            else if (workingName.EndsWith("-COPY")) workingName = workingName[..^5];
            else break;
        }
        return name.Substring(0, workingName.Length);
    }

    private static string ExtractFilenameFromUrl(string url)
    {
        var path = url.Split("?")[0];
        return path.Split("/").Last();
    }

    public static string DeriveExtensionFromFilename(string? filename)
    {
        if (!string.IsNullOrWhiteSpace(filename))
        {
            var ext = ThumbnailTools.ExtractExtensionFromFilename(filename).ToUpper();
            if (ext.StartsWith("PNG")) return "png";
            if (ext.StartsWith("JPEG") || ext.StartsWith("JPG")) return "jpg";
            if (ext.StartsWith("GIF")) return "gif";
            if (ext.StartsWith("BMP")) return "bmp";
        }
        return "png";
    }
    
    private int PrepareAdHierarchy(string channelAdSetId, string adSetName, string channelCampaignId,
        string campaignName, int attributionSetting, string campaignCreated, Channel channel)
    {
        var adSet = _adSetProxy.GetByChannelAdSetId(channelAdSetId);
        if (adSet == null)
        {
            adSet = CreateAdSet(channelAdSetId, adSetName, channelCampaignId, campaignName, attributionSetting, campaignCreated);
        }
        return adSet.Id;
    }

    private AdSet CreateAdSet(string channelAdSetId, string adSetName, string channelCampaignId,
        string campaignName, int attributionSetting, string campaignCreated)
    {
        var campaign = _campaignProxy.GetByChannelCampaignId(channelCampaignId);
        if (campaign == null)
        {
            campaign = CreateCampaign(channelCampaignId, campaignName, attributionSetting, campaignCreated, "");
        }

        var adSet = new AdSet
        {
            CampaignId = campaign.Id,
            ChannelAdSetId = channelAdSetId,
            Name = adSetName,
            Created = campaign.Created
        };

        _adSetProxy.Save(adSet);
        return adSet;
    }

    private Campaign CreateCampaign(string channelCampaignId, string campaignName, int attributionSetting,
        string campaignCreated, string objective)
    {
        var campaign = new Campaign
        {
            ChannelId = _channel.Id,
            ChannelCampaignId = channelCampaignId,
            Name = campaignName,
            Objective = objective,
            AttributionSetting = attributionSetting,
            Created = campaignCreated
        };

        _campaignProxy.Save(campaign);
        return campaign;
    }

    
    private Ad PrepareAd(int adSetId, FacebookAdCreative record)
    {
        var asset = PrepareAsset(record);
        Ad? ad = null;
        var ads = _adProxy.GetByChannelAdId(record.Id);
        if (ads != null && ads.Any())
        {
            ad = ads.FirstOrDefault(a => a.AssetId == asset.Id);
        }

        if (ad == null)
        {
            ad = new Ad
            {
                AdSetId = adSetId,
                AssetId = asset.Id,
                ChannelAdId = record.Id,
                Name = record.Name,
                DataAllyName = NormalizeNameFor(record.Name),
                AdCreated = record.CreatedTime,
                AdUpdated = record.UpdatedTime,
                AdDeactivated = record.Status.ToUpper() != "ACTIVE" ? record.UpdatedTime : null,
                Created = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _adProxy.Save(ad);
            CreateAdCopy(ad.Id, record.Creative.Title, record.Creative.Body);
        }
        else if ((ad.AdDeactivated != null && record.Status.ToUpper() == "ACTIVE") ||
                 (ad.AdDeactivated == null && record.Status.ToUpper() != "ACTIVE"))
        {
            ad.AdDeactivated = record.Status.ToUpper() != "ACTIVE" ? record.UpdatedTime : null;
            ad.Updated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            _adProxy.Save(ad);
        }

        var adMetadata = _adMetadataProxy.GetByAdId(ad.Id);
        if (adMetadata == null)
        {
            adMetadata = new AdMetadata
            {
                AdId = ad.Id,
                EffectiveDate = null,
                Status = record.Status,
                DestinationUrl = null,
                TrackingTemplateUrl = null,
                Utm = null,
                Created = ad.Created
            };
            _adMetadataProxy.Save(adMetadata);
        }
        else if (adMetadata.Status != record.Status)
        {
            adMetadata.Status = record.Status;
            adMetadata.Updated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            _adMetadataProxy.Save(adMetadata);
        }

        return ad;
    }

    private void CreateAdCopy(int adId, string? title, string? copy)
    {
        var cleanTitle = title ?? "";
        var cleanCopy = copy ?? "";
        var hashCode = HashTools.HashTitleAndBody(cleanTitle, cleanCopy);

        var existing = _adCopyProxy.GetByAdAndHashCode(adId, hashCode);
        if (existing == null)
        {
            var adCopy = new AdCopy
            {
                AdId = adId,
                Title = cleanTitle,
                Copy = cleanCopy,
                Hash = hashCode
            };
            _adCopyProxy.Save(adCopy);
        }
    }
}