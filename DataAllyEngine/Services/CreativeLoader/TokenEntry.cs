using FacebookLoader.Content;

namespace DataAllyEngine.Services.CreativeLoader;

public class TokenEntry
{
	public int CompanyId { get; set; }
	public int ChannelId { get; set; }
	public DateTime LastUsed { get; set; }
	
	public FacebookPageToken PageToken { get; set; }
	
	public TokenEntry(int companyId, int channelId, FacebookPageToken pageToken)
	{
		CompanyId = companyId;
		ChannelId = channelId;
		LastUsed = DateTime.UtcNow;
		PageToken = pageToken;
	}
}