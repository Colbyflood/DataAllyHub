using FacebookLoader.Content;

namespace DataAllyEngine.Services.CreativeLoader;

public class TokenEntry
{
	// ReSharper disable InconsistentNaming
	public const int EXPIRE_AFTER_DAYS = 15;
	
	public int CompanyId { get; }
	public string PageId { get; }
	public DateTime ExpirationDateUtc { get; }
	public DateTime LastUsedUtc { get; set; } 
	
	public FacebookPageToken PageToken { get; set; }
	
	public TokenEntry(int companyId, FacebookPageToken pageToken, DateTime expirationDate)
	{
		CompanyId = companyId;
		PageId = pageToken.PageId;
		PageToken = pageToken;
		ExpirationDateUtc = expirationDate;
		LastUsedUtc = DateTime.UtcNow;
	}
	
	public TokenKey GetKey()
	{
		return new TokenKey(CompanyId, PageId);
	}

	public bool IsExpired()
	{
		return ExpirationDateUtc > DateTime.UtcNow;
	}
}