using System.Data.Common;
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
        var Adset = contentProcessorProxy.GetAdsetByChannelAdsetId(channelAdSetId)
        if (Adset == null)
        {
            Adset = CreateAdSet(channelAdSetId, adSetName, channelCampaignId, campaignName, attributionSetting, campaignCreated);
        }
        return Adset.Id;
    }

    private Adset CreateAdSet(string channelAdSetId, string adSetName, string channelCampaignId,
        string campaignName, int attributionSetting, string campaignCreated)
    {
        var campaign = contentProcessorProxy.GetCampaignByChannelCampaignId(channelCampaignId);
        if (campaign == null)
        {
            campaign = CreateCampaign(channelCampaignId, campaignName, attributionSetting, campaignCreated, "");
        }

        var Adset = new Adset
        {
            CampaignId = campaign.Id,
            ChannelAdSetId = channelAdSetId,
            Name = adSetName,
            Created = campaign.Created
        };

        adSetProxy.Save(Adset);
        return Adset;
    }

    private Campaign CreateCampaign(string channelCampaignId, string campaignName, int attributionSetting,
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