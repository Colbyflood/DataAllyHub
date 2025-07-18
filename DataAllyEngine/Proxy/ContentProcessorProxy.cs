using DataAllyEngine.Common;
using DataAllyEngine.Context;
using DataAllyEngine.Models;

namespace DataAllyEngine.Proxy;

public class ContentProcessorProxy : IContentProcessorProxy
{
    private readonly DataAllyDbContext context;
    private readonly ILogger<IContentProcessorProxy> logger;

    public ContentProcessorProxy(DataAllyDbContext context, ILogger<IContentProcessorProxy> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public Channel? GetChannelById(int channelId)
    {
        return context.Channels.FirstOrDefault(c => c.Id == channelId);
    }

    public ChannelType? GetChannelTypeByName(string channelName)
    {
        return context.Channeltypes.FirstOrDefault(c => c.Name.ToLower() == channelName.ToLower());
    }

    public Client? GetClientById(int clientId)
    {
        return context.Clients.FirstOrDefault(c => c.Id == clientId);
    }

    public Account? GetAccountById(int accountId)
    {
        return context.Accounts.FirstOrDefault(a => a.Id == accountId);
    }

    public Company? GetCompanyById(int companyId)
    {
        return context.Companies.FirstOrDefault(c => c.Id == companyId);
    }

    public Company? GetCompanyByChannelId(int channelId)
    {
        var channel = context.Channels.FirstOrDefault(c => c.Id == channelId);
        if (channel == null)
        {
            return null;
        }
        var client = context.Clients.FirstOrDefault(c => c.Id == channel.ClientId);
        if (client == null)
        {
            return null;
        }
        var account = context.Accounts.FirstOrDefault(a => a.Id == client.AccountId);
        if (account == null)
        {
            return null;
        }
        return context.Companies.FirstOrDefault(c => c.Id == account.CompanyId);
    }

    public FbRunLog? GetFbRunLogById(int runlogId)
    {
        return context.Fbrunlogs.FirstOrDefault(f => f.Id == runlogId);
    }

    public List<FbSaveContent> LoadIncompleteSaveContentsAfter(DateTime startDateUtc)
    {
        return context.Fbsavecontents
            .Where(s => s.AdCreativeFinishedUtc == null || s.AdImageFinishedUtc == null || s.AdInsightFinishedUtc == null)
            .Where(s => s.QueuedUtc >= startDateUtc)
            .ToList();
    }

    public FbSaveContent? LoadFbSaveContentContainingRunlog(int runlogId)
    {
        return context.Fbsavecontents.FirstOrDefault(f => f.AdCreativeRunlogId == runlogId || f.AdImageRunlogId == runlogId || f.AdInsightRunlogId == runlogId);
    }

    public FbSaveContent? LoadFbSaveContentById(int id)
    {
        return context.Fbsavecontents.FirstOrDefault(f => f.Id == id);
    }

    public void WriteFbSaveContent(FbSaveContent saveContent)
    {
        if (saveContent.Id <= 0)
        {
            context.Fbsavecontents.Add(saveContent);
        }
        context.SaveChanges();
    }

    public void WriteFbSaveContentHeartBeat(int id)
    {
        FbSaveContent? fbSaveContent = LoadFbSaveContentById(id);
        if (fbSaveContent != null)
        {
            var utcNow = DateTime.UtcNow;

            if (fbSaveContent.HeartBeatLastReceivedAtUtc != null)
            {
                var lastHeartBeatUtc = DateTime.SpecifyKind(fbSaveContent.HeartBeatLastReceivedAtUtc.Value, DateTimeKind.Utc);
                var diff = (utcNow - lastHeartBeatUtc).TotalSeconds;
                if (diff < 15)
                {
                    return;
                }
            }

            fbSaveContent.HeartBeatLastReceivedAtUtc = utcNow;
            WriteFbSaveContent(fbSaveContent);
        }
    }

    public List<FbRunStaging> LoadFbRunStagingForRunlog(int runlogId)
    {
        return context.Fbrunstagings.Where(s => s.FbRunlogId == runlogId).ToList();
    }

    public void DeleteFbRunStaging(FbRunStaging staging)
    {
        context.Fbrunstagings.Remove(staging);
        context.SaveChanges();
    }

    public List<Ad> GetAdsByChannelAdIdAndChannelId(string channelAdId, int channelId)
    {
        return context.Ads.Where(a =>
                                     a.ChannelAdId == channelAdId
                                     &&
                                     a.Adset.Campaign.ChannelId == channelId
                                ).ToList();
    }

    public Adset? GetAdsetByChannelAdsetIdAndChannelId(string channelAdsetId, int channelId)
    {
        return context.Adsets.FirstOrDefault(a =>
                                                a.ChannelAdsetId.ToLower() == channelAdsetId.ToLower()
                                                &&
                                                a.Campaign.ChannelId == channelId
                                                );
    }

    public void WriteAdset(Adset adset)
    {
        if (adset.Id <= 0)
        {
            context.Adsets.Add(adset);
        }
        context.SaveChanges();
    }

    public Campaign? GetCampaignByChannelCampaignIdAndChannelId(string channelCampaignId, int channelId)
    {
        return context.Campaigns.FirstOrDefault(c =>
                                                    c.ChannelCampaignId.ToLower() == channelCampaignId.ToLower()
                                                    &&
                                                    c.ChannelId == channelId
                                                );
    }

    public void WriteCampaign(Campaign campaign)
    {
        if (campaign.Id <= 0)
        {
            context.Campaigns.Add(campaign);
        }
        context.SaveChanges();
    }

    public Ad? GetAdByChannelAdId(string channelAdId, int channelId)
    {
        return context.Ads.FirstOrDefault(a =>
                                            a.ChannelAdId.ToLower() == channelAdId.ToLower()
                                            &&
                                            a.Adset.Campaign.ChannelId == channelId
                                         );
    }

    public void WriteAd(Ad ad)
    {
        if (ad.Id <= 0)
        {
            context.Ads.Add(ad);
        }
        context.SaveChanges();
    }

    public AdMetadata? GetAdMetadataByAdId(int adId)
    {
        return context.Admetadatas.FirstOrDefault(a => a.AdId == adId);
    }

    public void WriteAdMetadata(AdMetadata adMetadata)
    {
        if (adMetadata.Id <= 0)
        {
            context.Admetadatas.Add(adMetadata);
        }
        context.SaveChanges();
    }

    public List<AdCopy> GetAdCopyByAdId(int adId)
    {
        return context.Adcopies.Where(a => a.AdId == adId).ToList();
    }

    public AdCopy? GetAdCopyByAdIdAndHashCode(int adId, string hashCode)
    {
        return context.Adcopies.FirstOrDefault(a => a.AdId == adId && a.Hash.ToLower() == hashCode.ToLower());
    }

    public void WriteAdCopy(AdCopy adCopy)
    {
        if (adCopy.Id <= 0)
        {
            context.Adcopies.Add(adCopy);
        }
        context.SaveChanges();
    }

    public Asset? GetAssetByChannelIdAndKey(int channelId, string key)
    {
        return context.Assets.FirstOrDefault(a => a.ChannelId == channelId && a.AssetKey.ToLower() == key.ToLower());

    }

    public Asset? GetAssetByChannelIdTypeNameAndKey(int channelId, string assetTypeName, string key)
    {
        return context.Assets.FirstOrDefault(a =>
                                                   a.ChannelId == channelId
                                                && a.AssetType.ToLower() == assetTypeName.ToLower()
                                                && a.AssetKey.ToLower() == key.ToLower()
                                            );
    }

    public void WriteAsset(Asset asset)
    {
        if (asset.Id <= 0)
        {
            context.Assets.Add(asset);
        }
        context.SaveChanges();
    }

    public Thumbnail? GetThumbnailByFilenameAndChannelAdId(string filename, string channelAdId, int channelId)
    {
        return context.Thumbnails.FirstOrDefault(t =>
                                                    t.ChannelAdId.ToLower() == channelAdId.ToLower()
                                                 && t.Filename.ToLower() == filename.ToLower()
                                                 && t.ChannelId == channelId
                                                );
    }

    public void WriteThumbnail(Thumbnail thumbnail)
    {
        if (thumbnail.Id <= 0)
        {
            context.Thumbnails.Add(thumbnail);
        }
        context.SaveChanges();
    }

    public FbCreativeLoad? GetFbCreativeLoadByImageHash(string imageHash, int channelId)
    {
        return context.Fbcreativeloads
            .SingleOrDefault(rec =>
                                    rec.ChannelId == channelId
                                 && rec.CreativeKey.ToUpper() == imageHash.ToUpper()
                                 && rec.CreativeType == Names.CREATIVE_TYPE_IMAGE
                                 );
    }

    public FbCreativeLoad? GetFbCreativeLoadByVideoId(string videoId, int channelId)
    {
        return context.Fbcreativeloads
            .SingleOrDefault(rec =>
                                    rec.ChannelId == channelId
                                 && rec.CreativeKey.ToUpper() == videoId.ToUpper()
                                 && rec.CreativeType == Names.CREATIVE_TYPE_VIDEO
                                 );
    }

    public void WriteFbCreativeLoad(FbCreativeLoad record)
    {
        if (record.Id <= 0)
        {
            context.Fbcreativeloads.Add(record);
        }
        context.SaveChanges();
    }

}