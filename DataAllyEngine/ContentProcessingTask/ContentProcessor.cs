using System.Drawing;
using System.Net.Mime;
using Amazon.S3;
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
    private readonly IAmazonS3 s3Client;
    private readonly ILogger<IContentProcessor> logger;

    public ContentProcessor(IContentProcessorProxy contentProcessorProxy, IKpiProxy kpiProxy, IConfigurationLoader configurationLoader,
        IAmazonS3 s3Client, ILogger<IContentProcessor> logger)
    {
        this.contentProcessorProxy = contentProcessorProxy;
        this.kpiProxy = kpiProxy;
        this.s3Client = s3Client;
        thumbnailBucket = configurationLoader.GetKeyValueFor(Names.THUMBNAIL_BUCKET_KEY);
        this.logger = logger;
    }

    public void ProcessContentFor(Channel channel, FbRunLog runlog)
    {
        if (runlog.FeedType == Names.FEED_TYPE_AD_INSIGHT)
        {
            ProcessAdInsights(channel, runlog);
        }
        else if (runlog.FeedType == Names.FEED_TYPE_AD_CREATIVE)
        {
            ProcessAdCreatives(channel, runlog);
        }
        else
        {
            ProcessAdImages(channel, runlog);
        }
    }

    private static DateTime ParseDate(string dateString)
    {
        if (DateTime.TryParse(dateString, out var dateTime))
        {
            return dateTime.Date;
        }
        else
        {
            throw new FormatException("Invalid effective date");
        }
    }

    private void ProcessAdCreatives(Channel channel, FbRunLog runlog)
    {
        var company = ExtractCompanyFromChannel(channel);
        var stagingEntries = contentProcessorProxy.LoadFbRunStagingForRunlog(runlog.Id);
        foreach (var entry in stagingEntries)
        {
            Console.WriteLine("Deserializing a staging entry for ad creatives");

            var facebookAdCreativesResponse = FacebookAdCreativesResponse.FromJson(entry.Content);
            if (facebookAdCreativesResponse == null)
            {
                logger.LogWarning($"Empty facebook ad creatives response for runlog {runlog.Id}");
                continue;
            }
            foreach (var record in facebookAdCreativesResponse.Content)
            {
                var adsetId = PrepareAdHierarchy(record.AdsetId, $"Adset {record.AdsetId}",
                                                 record.CampaignId, $"Campaign {record.CampaignId}",
                                                 0, record.CreatedTime, channel);
                PrepareAd(adsetId, company, channel, record);
            }
        }

        var saveContent = contentProcessorProxy.LoadFbSaveContentContainingRunlog(runlog.Id);
        if (saveContent != null)
        {
            saveContent.AdCreativeFinishedUtc = DateTime.UtcNow;
            contentProcessorProxy.WriteFbSaveContent(saveContent);
        }

        CleanStaging(stagingEntries);
    }

    private Company ExtractCompanyFromChannel(Channel channel)
    {
        var client = contentProcessorProxy.GetClientById(channel.ClientId);
        var account = contentProcessorProxy.GetAccountById(client!.AccountId);
        return contentProcessorProxy.GetCompanyById(account!.CompanyId)!;
    }

    private void ProcessAdImages(Channel channel, FbRunLog runlog)
    {
        var stagingEntries = contentProcessorProxy.LoadFbRunStagingForRunlog(runlog.Id);
        Console.WriteLine("Deserializing a staging entry for ad images");
        foreach (var entry in stagingEntries)
        {
            var facebookAdImagesResponse = FacebookAdImagesResponse.FromJson(entry.Content);
            if (facebookAdImagesResponse == null)
            {
                logger.LogWarning($"Empty facebook ad images response for runlog {runlog.Id}");
                continue;
            }
            foreach (var record in facebookAdImagesResponse.Content)
            {
                PrepareImages(channel, record);
            }
        }

        var saveContent = contentProcessorProxy.LoadFbSaveContentContainingRunlog(runlog.Id);
        if (saveContent != null)
        {
            saveContent.AdImageFinishedUtc = DateTime.UtcNow;
            contentProcessorProxy.WriteFbSaveContent(saveContent);
        }

        CleanStaging(stagingEntries);
    }

    private void PrepareImages(Channel channel, FacebookAdImage record)
    {
        foreach (var creativeId in record.Creatives)
        {
            var asset = contentProcessorProxy.GetAssetByChannelIdAndKey(channel.Id, creativeId);
            if (asset == null)
            {
                Console.WriteLine($"[WARN] Encountered image in channel {channel.Id} without corresponding asset Id in creatives array: {creativeId}");
            }
            else
            {
                var savedName = asset.AssetName ?? string.Empty;
                var savedUrl = asset.Url ?? string.Empty;
                var recordName = record.Name ?? string.Empty;
                var recordUrl = !string.IsNullOrWhiteSpace(record.Url128) ? record.Url128 : (record.Url ?? string.Empty);
                if (!savedName.Equals(recordName, StringComparison.OrdinalIgnoreCase) ||
                    !savedUrl.Equals(recordUrl, StringComparison.OrdinalIgnoreCase))
                {
                    asset.AssetName = recordName;
                    asset.Url = recordUrl;
                    contentProcessorProxy.WriteAsset(asset);
                }
            }
        }
    }


    private void ProcessAdInsights(Channel channel, FbRunLog runlog)
    {
        var stagingEntries = contentProcessorProxy.LoadFbRunStagingForRunlog(runlog.Id);
        if (stagingEntries == null || !stagingEntries.Any()) return;

        var kpiProcessor = new KpiProcessor(channel, kpiProxy, logger);
        foreach (var entry in stagingEntries)
        {
            Console.WriteLine("Deserializing a staging entry for ad insights");
            var facebookAdInsightsResponse = FacebookAdInsightsResponse.FromJson(entry.Content);
            if (facebookAdInsightsResponse == null)
            {
                logger.LogWarning($"Empty facebook ad insights response for runlog {runlog.Id}");
                continue;
            }
            foreach (var record in facebookAdInsightsResponse.Content)
            {
                PrepareKpis(channel, kpiProcessor, record);
            }
        }

        var saveContent = contentProcessorProxy.LoadFbSaveContentContainingRunlog(runlog.Id);
        if (saveContent != null)
        {
            saveContent.AdInsightFinishedUtc = DateTime.UtcNow;
            contentProcessorProxy.WriteFbSaveContent(saveContent);
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
            Created = ParseDate(campaignCreated)
        };

        contentProcessorProxy.WriteCampaign(campaign);
        return campaign;
    }


    private Ad PrepareAd(int adSetId, Company company, Channel channel, FacebookAdCreative record)
    {
        var asset = PrepareAsset(channel, record);
        Ad? ad = null;
        var ads = contentProcessorProxy.GetAdsByChannelAdId(record.Id);
        if (ads != null && ads.Any())
        {
            ad = ads.FirstOrDefault(a => a.AssetId == asset.Id);
        }

        if (ad == null)
        {
            ad = new Ad
            {
                AdsetId = adSetId,
                AssetId = asset.Id,
                ChannelAdId = record.Id,
                Name = record.Name,
                DataallyName = NormalizeNameFor(record.Name),
                AdCreated = ParseDate(record.CreatedTime),
                AdUpdated = ParseDate(record.UpdatedTime),
                AdDeactivated = record.Status.ToUpper() != "ACTIVE" ? ParseDate(record.UpdatedTime) : null,
                Created = DateTime.UtcNow
            };

            contentProcessorProxy.WriteAd(ad);
            CreateAdCopy(ad.Id, record.Creative.Title, record.Creative.Body);
        }
        else if ((ad.AdDeactivated != null && record.Status.ToUpper() == "ACTIVE") ||
                 (ad.AdDeactivated == null && record.Status.ToUpper() != "ACTIVE"))
        {
            ad.AdDeactivated = record.Status.ToUpper() != "ACTIVE" ? ParseDate(record.UpdatedTime) : null;
            ad.Updated = DateTime.UtcNow;
            contentProcessorProxy.WriteAd(ad);
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
            contentProcessorProxy.WriteAdMetadata(adMetadata);
        }
        else if (adMetadata.Status != record.Status)
        {
            adMetadata.Status = record.Status;
            adMetadata.Updated = DateTime.UtcNow;
            contentProcessorProxy.WriteAdMetadata(adMetadata);
        }

        QueueVideosAndImages(company, channel, record);

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
                Created = ParseDate(record.CreatedTime),
                Updated = ParseDate(record.UpdatedTime)
            };

            contentProcessorProxy.WriteAsset(asset);
        }

        ProcessThumbnail(asset, record.Id);

        return asset;
    }

    private void QueueVideosAndImages(Company company, Channel channel, FacebookAdCreative record)
    {
        QueueImageLoader(ExtractImageHashes(record.Creative), company, channel, record.Id);
        QueueVideoLoader(ExtractVideoIds(record.Creative), company, channel, record.Id);
    }

    private void QueueImageLoader(List<CreativeImageHash> hashes, Company company, Channel channel, string channelAdId)
    {
        foreach (var hash in hashes)
        {
            var existingFbCreativeLoad = contentProcessorProxy.GetFbCreativeLoadByImageHash(hash.Hash);

            if (existingFbCreativeLoad != null)
            {
                if (existingFbCreativeLoad.CarouselOrderNo != hash.carouselOrderNo)
                {
                    existingFbCreativeLoad.CarouselOrderNo = hash.carouselOrderNo;
                    contentProcessorProxy.WriteFbCreativeLoad(existingFbCreativeLoad);
                }

                continue;
            }
            else
            {
                var record = new FbCreativeLoad()
                {
                    CreativeKey = hash.Hash,
                    CreativeType = Names.CREATIVE_TYPE_IMAGE,
                    CreativePageId = hash.PageId,
                    CompanyId = company.Id,
                    ChannelId = channel.Id,
                    ChannelAdId = channelAdId,
                    CreatedDateTimeUtc = DateTime.UtcNow,
                    TotalAttempts = 0,
                    CarouselOrderNo = hash.carouselOrderNo
                };

                contentProcessorProxy.WriteFbCreativeLoad(record);
            }
        }
    }

    private void QueueVideoLoader(List<CreativeVideoId> videoIds, Company company, Channel channel, string channelAdId)
    {
        foreach (var videoId in videoIds)
        {
            if (contentProcessorProxy.GetFbCreativeLoadByVideoId(videoId.VideoId) != null)
            {
                continue;
            }

            var record = new FbCreativeLoad()
            {
                CreativeKey = videoId.VideoId,
                CreativeType = Names.CREATIVE_TYPE_VIDEO,
                CreativePageId = videoId.PageId,
                CompanyId = company.Id,
                ChannelId = channel.Id,
                ChannelAdId = channelAdId,
                CreatedDateTimeUtc = DateTime.UtcNow,
                TotalAttempts = 0
            };

            contentProcessorProxy.WriteFbCreativeLoad(record);
        }
    }

    private List<CreativeImageHash> ExtractImageHashes(FacebookCreative record)
    {
        var imageHashes = new List<CreativeImageHash>();
        string defaultPageId = string.IsNullOrWhiteSpace(record.PageId) ? "0" : record.PageId;
        if (!string.IsNullOrWhiteSpace(record.PhotoData.ImageHash) && !string.IsNullOrWhiteSpace(record.PhotoData.PageId))
        {
            imageHashes.Add(new CreativeImageHash(record.PhotoData.ImageHash, record.PhotoData.PageId));
        }

        if (!string.IsNullOrWhiteSpace(record.ImageHash))
        {
            imageHashes.Add(new CreativeImageHash(record.ImageHash, defaultPageId));
        }


        if (!string.IsNullOrWhiteSpace(record.LinkData.ImageHash) && !string.IsNullOrWhiteSpace(record.LinkData.PageId))
        {
            imageHashes.Add(new CreativeImageHash(record.LinkData.ImageHash, record.LinkData.PageId));
        }

        // CrousalImages came in child attachments
        if (record.LinkData.ChildAttachments != null & record.LinkData.ChildAttachments.Count > 0)
        {
            int crousalImageOrder = 1;
            record.LinkData.ChildAttachments.ForEach(attachment =>
            {
                if (!string.IsNullOrWhiteSpace(attachment.ImageHash))
                {
                    imageHashes.Add(new CreativeImageHash(attachment.ImageHash, defaultPageId, crousalImageOrder++));
                }
            });
        }

        return imageHashes;

        // $images = [];
        // // 1. child_attachments[].image_hash
        // foreach (data_get($this->creative, 'object_story_spec.link_data.child_attachments', []) as $item) {
        //     $images[] = $this->getImageByHash(data_get($item, 'image_hash'));
        // }
        //
        // // 2. link_data.image_hash
        // if ($image = $this->getImageByHash(data_get($this->creative, 'object_story_spec.link_data.image_hash'))) {
        //     $images[] = $image;
        // }
        //
        // // 3. asset_feed_spec.images[].hash
        // $dynamicImages = collect(data_get($this->creative, 'asset_feed_spec.images'));
        // if ($dynamicImages->isNotEmpty()) {
        //     $images = array_merge($images, $dynamicImages->map(function ($image) {
        //         return $this->getImageByHash(data_get($image, 'hash'));
        //     })->toArray());
        // }
        //
        // // 4. photo_data.image_hash
        // if ($hash = data_get($this->creative, 'object_story_spec.photo_data.image_hash')) {
        //     $images[] = $this->getImageByHash($hash);
        // }
        //
        // // 5. root-level image_hash
        // if ($rootImageHash = data_get($this->creative, 'image_hash')) {
        //     $images[] = $this->getImageByHash($rootImageHash);
        // }
        //
        // return array_filter($images); // Remove nulls
    }

    private List<CreativeVideoId> ExtractVideoIds(FacebookCreative record)
    {
        var videoIds = new List<CreativeVideoId>();
        string? defaultPageId = record.PageId;
        if (!string.IsNullOrWhiteSpace(record.VideoData.VideoId) && !string.IsNullOrWhiteSpace(record.VideoData.PageId))
        {
            videoIds.Add(new CreativeVideoId(record.VideoData.VideoId, record.VideoData.PageId));
            defaultPageId = record.VideoData.PageId;
        }

        if (!string.IsNullOrWhiteSpace(record.VideoId) && !string.IsNullOrWhiteSpace(defaultPageId))
        {
            videoIds.Add(new CreativeVideoId(record.VideoId, defaultPageId));
        }
        return videoIds;
        // $videoIds = [];
        // // 1. object_story_spec.video_data.video_id
        // if ($videoId = data_get($this->creative, 'object_story_spec.video_data.video_id')) {
        //     $videoIds = Arr::wrap($videoId);
        // }
        // // 2. asset_feed_spec.videos[].video_id
        // if ($videos = data_get($this->creative, 'asset_feed_spec.videos', [])) {
        //     $videoIds = array_merge($videoIds, collect($videos)->map(function ($video) {
        //         return data_get($video, 'video_id');
        //     })->toArray());
        // }
        // // 3. root-level video_id
        // if ($rootVideoId = data_get($this->creative, 'video_id')) {
        //     $videoIds = array_merge($videoIds, Arr::wrap($rootVideoId));
        // }
        //
        // $videoIds = array_filter($videoIds); // Remove nulls/empties
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

        MemoryStream? imageStream = null;
        try
        {
            imageStream = ImageStorageTools.FetchFileToMemory(asset.Url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error downloading {asset.Url} for asset {asset.AssetType} {asset.AssetKey}: {ex}");
        }

        if (imageStream == null)
        {
            Console.WriteLine($"[WARN] No image for asset {asset.AssetType} {asset.AssetKey}");
            return null;
        }

        var uuid = ImageStorageTools.GenerateGuid();
        var binId = ImageStorageTools.HashThumbnailToBinId(uuid);

        try
        {
            var extension = ImageStorageTools.DeriveExtensionFromFilename(filename);
            var s3Key = ImageStorageTools.AssembleS3Key(uuid, extension, binId);
            //var imageBytes = ThumbnailTools.ConvertImageToBytes(image);
            ImageStorageTools.SaveImageToS3(s3Client, imageStream, thumbnailBucket, s3Key);

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

    private void PrepareKpis(Channel channel, KpiProcessor kpiProcessor, FacebookAdInsight record)
    {
        var ads = contentProcessorProxy.GetAdsByChannelAdId(record.Id);
        if (ads == null || !ads.Any())
        {
            Console.WriteLine($"[WARN] Cannot find an Ad entry for KPI referencing ad Id {record.Id} in channel {channel.Id}");
            return;
        }

        var ad = ads[0];

        foreach (var insight in record.Insights)
        {
            var campaign = UpdateCampaign(channel, insight.CampaignId, insight.CampaignName, insight.Objective, record.CreatedTime);
            UpdateAdSet(insight.AdsetId, insight.AdsetName, record.CreatedTime, campaign);
            kpiProcessor.ImportKpis(ad, insight);
        }
    }

    private Campaign UpdateCampaign(Channel channel, string campaignId, string campaignName, string objective, string createdDate)
    {
        var campaign = contentProcessorProxy.GetCampaignByChannelCampaignId(campaignId);
        if (campaign == null)
        {
            return CreateCampaign(channel, campaignId, campaignName, 0, createdDate, objective);
        }

        var savedName = campaign.Name;
        var savedObjective = campaign.Objective ?? string.Empty;
        var recordName = campaignName ?? string.Empty;
        var recordObjective = objective ?? string.Empty;

        if (!savedName.Equals(recordName, StringComparison.OrdinalIgnoreCase) ||
            !savedObjective.Equals(recordObjective, StringComparison.OrdinalIgnoreCase))
        {
            campaign.Name = campaignName;
            campaign.Objective = objective;
            contentProcessorProxy.WriteCampaign(campaign);
        }

        return campaign;
    }

    private Adset UpdateAdSet(string adSetId, string adSetName, string createdDate, Campaign campaign)
    {
        var adSet = contentProcessorProxy.GetAdsetByChannelAdsetId(adSetId);
        if (adSet == null)
        {
            adSet = new Adset
            {
                CampaignId = campaign.Id,
                ChannelAdsetId = adSetId,
                Name = adSetName,
                Created = ParseDate(createdDate)
            };
            contentProcessorProxy.WriteAdset(adSet);
            return adSet;
        }

        if (!string.Equals(adSet.Name, adSetName, StringComparison.OrdinalIgnoreCase))
        {
            adSet.Name = adSetName;
            contentProcessorProxy.WriteAdset(adSet);
        }

        return adSet;
    }
}