using FacebookLoader.Content;

namespace DataAllyEngine.Services.CreativeLoader;

public class TokenEntry
{
	// ReSharper disable InconsistentNaming
	public const int EXPIRE_AFTER_DAYS = 15;
	
	public int CompanyId { get; }
	public int ChannelId { get; }
	public string PageId { get; }
	public DateTime ExpirationDateUtc { get; }
	public DateTime LastUsedUtc { get; set; } 
	
	public FacebookPageToken PageToken { get; set; }
	
	public TokenEntry(int companyId, int channelId, FacebookPageToken pageToken, DateTime expirationDate)
	{
		CompanyId = companyId;
		ChannelId = channelId;
		PageToken = pageToken;
		ExpirationDateUtc = expirationDate;
		LastUsedUtc = DateTime.UtcNow;
	}
	
	public TokenKey GetKey()
	{
		return new TokenKey(CompanyId, ChannelId, PageId);
	}

	public bool IsExpired()
	{
		return ExpirationDateUtc > DateTime.UtcNow;
	}
}