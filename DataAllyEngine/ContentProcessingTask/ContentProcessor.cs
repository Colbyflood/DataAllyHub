using System.Data.Common;
using System.Drawing;
using System.Net;
using DataAllyEngine.Common;
using DataAllyEngine.Configuration;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Content;

namespace DataAllyEngine.ContentProcessingTask;

public class ContentProcessor : IContentProcessor
{
    private readonly string thumbnailBucket;

    private readonly IContentProcessorProxy contentProcessorProxy;
    private readonly IKpiProxy kpiProxy;
    private readonly ILogger<IContentProcessor> logger;
    
    public ContentProcessor(IContentProcessorProxy contentProcessorProxy, IKpiProxy kpiProxy, IConfigurationLoader configurationLoader, ILogger<IContentProcessor> logger)
    {
        this.contentProcessorProxy = contentProcessorProxy;
        this.kpiProxy = kpiProxy;
        thumbnailBucket = configurationLoader.GetKeyValueFor(Names.THUMBNAIL_BUCKET_KEY);
        this.logger = logger;
    }

    public void ProcessContentFor(Channel channel)
    {

    }

    private void Process(FbRunLog runlog)
    {
        if (runlog.FeedType == Names.FEED_TYPE_AD_INSIGHT)
        {
            ProcessAdInsights(runlog);
        }
        else if (runlog.FeedType == Names.FEED_TYPE_AD_CREATIVE)
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
        var stagingEntries = contentProcessorProxy.LoadFbRunStagingForRunlog(runlog.Id);
        foreach (var entry in stagingEntries)
        {
            Console.WriteLine("Deserializing a staging entry for ad creatives");
            foreach (var record in FacebookAdCreativeTools.Deserialize(entry.Content))
            {
                var adsetId = PrepareAdHierarchy(record.AdSetId, $"Adset {record.AdSetId}",
                                                 record.CampaignId, $"Campaign {record.CampaignId}",
                                                 0, record.CreatedTime, channel);
                PrepareAd(adsetId, record);
            }
        }
        CleanStaging(stagingEntries);
    }

    private void ProcessAdImages(FbRunLog runlog)
    {
        var stagingEntries = contentProcessorProxy.LoadFbRunStagingForRunlog(runlog.Id);
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
        var stagingEntries = contentProcessorProxy.LoadFbRunStagingForRunlog(runlog.Id);
        if (stagingEntries == null || !stagingEntries.Any()) return;

        var kpiProcessor = new KpiProcessor(channel, dbConnection);
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
            contentProcessorProxy.DeleteFbRunStaging(entry);
        }
    }

    private static string NormalizeNameFor(string name)
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
        var Adset = contentProcessorProxy.GetAdsetByChannelAdsetId(channelAdSetId);
        if (Adset == null)
        {
            Adset = CreateAdSet(channel, channelAdSetId, adSetName, channelCampaignId, campaignName, attributionSetting, campaignCreated);
        }
        return Adset.Id;
    }

    private Adset CreateAdSet(Channel channel, string channelAdSetId, string adSetName, string channelCampaignId,
        string campaignName, int attributionSetting, string campaignCreated)
    {
        var campaign = contentProcessorProxy.GetCampaignByChannelCampaignId(channelCampaignId);
        if (campaign == null)
        {
            campaign = CreateCampaign(channel, channelCampaignId, campaignName, attributionSetting, campaignCreated, "");
        }

        var Adset = new Adset
        {
            CampaignId = campaign.Id,
            ChannelAdsetId = channelAdSetId,
            Name = adSetName,
            Created = campaign.Created
        };

        contentProcessorProxy.WriteAdset(Adset);
        return Adset;
    }

    private Campaign CreateCampaign(Channel channel, string channelCampaignId, string campaignName, int attributionSetting,
        string campaignCreated, string objective)
    {
        var campaign = new Campaign
        {
            ChannelId = channel.Id,
            ChannelCampaignId = channelCampaignId,
            Name = campaignName,
            Objective = objective,
            AttributionSetting = attributionSetting,
            Created = campaignCreated
        };

        contentProcessorProxy.WriteCampaign(campaign);
        return campaign;
    }

    
    private Ad PrepareAd(int adSetId, FacebookAdCreative record)
    {
        var asset = PrepareAsset(record);
        Ad? ad = null;
        var ads = contentProcessorProxy.GetAdsByChannelAdId(record.Id)
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

            contentProcessorProxy.WriteAd(ad);
            CreateAdCopy(ad.Id, record.Creative.Title, record.Creative.Body);
        }
        else if ((ad.AdDeactivated != null && record.Status.ToUpper() == "ACTIVE") ||
                 (ad.AdDeactivated == null && record.Status.ToUpper() != "ACTIVE"))
        {
            ad.AdDeactivated = record.Status.ToUpper() != "ACTIVE" ? record.UpdatedTime : null;
            ad.Updated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            adProxy.Save(ad);
        }

        var adMetadata = contentProcessorProxy.GetAdMetadataByAdId(ad.Id);
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
            adMetadataProxy.Save(adMetadata);
        }
        else if (adMetadata.Status != record.Status)
        {
            adMetadata.Status = record.Status;
            adMetadata.Updated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            contentProcessorProxy.WriteAdMetadata(adMetadata);
        }

        return ad;
    }
    
    private Asset PrepareAsset(Channel channel, FacebookAdCreative record)
    {
        var assetType = record.Creative.ObjectType.ToUpper();
        var asset = contentProcessorProxy.GetAssetByChannelIdTypeNameAndKey(channel.Id, assetType, record.Creative.Id);

        if (asset == null)
        {
            asset = new Asset
            {
                AssetType = assetType,
                AssetKey = record.Creative.Id.ToUpper(),
                ChannelId = channel.Id,
                Url = record.Creative.ThumbnailUrl,
                Created = record.CreatedTime,
                Updated = record.UpdatedTime
            };

            contentProcessorProxy.WriteAsset(asset);
        }

        ProcessThumbnail(asset, record.Id);

        return asset;
    }
    
    private Thumbnail? ProcessThumbnail(Asset asset, string channelAdId)
    {
        if (string.IsNullOrWhiteSpace(asset.Url) || !asset.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[WARN] Unable to process image for asset {asset.AssetType} {asset.AssetKey} because url is empty or not http");
            return null;
        }

        var filename = ExtractFilenameFromUrl(asset.Url);
        var thumbnail = contentProcessorProxy.GetThumbnailByFilenameAndChannelAdId(filename, channelAdId);
        if (thumbnail != null)
            return thumbnail;

        Image? image = null;
        try
        {
            image = ThumbnailTools.FetchThumbnail(asset.Url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error downloading {asset.Url} for asset {asset.AssetType} {asset.AssetKey}: {ex}");
        }

        if (image == null)
        {
            Console.WriteLine($"[WARN] No image for asset {asset.AssetType} {asset.AssetKey}");
            return null;
        }

        var uuid = ThumbnailTools.GenerateGuid();
        var binId = ThumbnailTools.HashToBinId(uuid);

        try
        {
            var extension = DeriveExtensionFromFilename(filename);
            var s3Key = ThumbnailTools.AssembleS3Key(uuid, extension, binId);
            var imageBytes = ThumbnailTools.ConvertImageToBytes(image);
            ThumbnailTools.SaveThumbnail(imageBytes, thumbnailBucket, s3Key);

            thumbnail = new Thumbnail
            {
                ChannelAdId = channelAdId,
                Filename = filename,
                Guid = uuid,
                BinId = binId,
                Extension = extension
            };

            contentProcessorProxy.WriteThumbnail(thumbnail);

            asset.ThumbnailGuid = uuid;
            contentProcessorProxy.WriteAsset(asset);

            return thumbnail;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unable to save thumbnail for {filename} as {uuid} for {asset.AssetType} {asset.AssetKey}: {ex}");
            return null;
        }
    }


    private void CreateAdCopy(int adId, string? title, string? copy)
    {
        var cleanTitle = title ?? "";
        var cleanCopy = copy ?? "";
        var hashCode = HashTools.HashTitleAndBody(cleanTitle, cleanCopy);

        var existing = contentProcessorProxy.GetAdCopyByAdIdAndHashCode(adId, hashCode);
        if (existing == null)
        {
            var adCopy = new AdCopy
            {
                AdId = adId,
                Title = cleanTitle,
                Copy = cleanCopy,
                Hash = hashCode
            };
            contentProcessorProxy.WriteAdCopy(adCopy);
        }
    }
}