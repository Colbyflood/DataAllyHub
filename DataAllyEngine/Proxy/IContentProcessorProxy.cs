using DataAllyEngine.Models;

namespace DataAllyEngine.Proxy;

public interface IContentProcessorProxy
{
	Channel? GetChannelById(int channelId);

	ChannelType? GetChannelTypeByName(string channelName);
	
	Client? GetClientById(int clientId);
	
	Account? GetAccountById(int accountId);
	
	Company? GetCompanyById(int companyId);
	Company? GetCompanyByChannelId(int channelId);
	
	FbRunLog? GetFbRunLogById(int runlogId);
	
	List<FbSaveContent> LoadIncompleteSaveContentsAfter(DateTime startDateUtc);
	FbSaveContent? LoadFbSaveContentContainingRunlog(int runlogId);
	void WriteFbSaveContent(FbSaveContent saveContent);

	List<FbRunStaging> LoadFbRunStagingForRunlog(int runlogId);
	void DeleteFbRunStaging(FbRunStaging staging);

	List<Ad> GetAdsByChannelAdId(string channelAdId);
	
	Adset? GetAdsetByChannelAdsetId(string channelAdsetId);
	void WriteAdset(Adset adset);
	
	Campaign? GetCampaignByChannelCampaignId(string channelCampaignId);
	void WriteCampaign(Campaign campaign);
	
	Ad? GetAdByChannelAdId(string channelAdId);
	void WriteAd(Ad ad);
	
	AdMetadata? GetAdMetadataByAdId(int adId);
	void WriteAdMetadata(AdMetadata adMetadata);
	
	List<AdCopy> GetAdCopyByAdId(int adId);
	AdCopy? GetAdCopyByAdIdAndHashCode(int adId, string hashCode);
	void WriteAdCopy(AdCopy adCopy);

	Asset? GetAssetByChannelIdAndKey(int channelId, string key);
	Asset? GetAssetByChannelIdTypeNameAndKey(int channelId, string assetTypeName, string key);
	void WriteAsset(Asset asset);

	Thumbnail? GetThumbnailByFilenameAndChannelAdId(string filename, string channelAdId);
	void WriteThumbnail(Thumbnail thumbnail);
}