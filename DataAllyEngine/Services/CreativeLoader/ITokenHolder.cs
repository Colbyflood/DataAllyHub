using FacebookLoader.Content;

namespace DataAllyEngine.Services.CreativeLoader;

public interface ITokenHolder
{
	Task<FacebookPageToken?> GetFacebookPageToken(TokenKey key);
	void ClearExpiredEntries();
}