using DataAllyEngine.Proxy;
using FacebookLoader.Common;
using FacebookLoader.Content;

namespace DataAllyEngine.Services.CreativeLoader;

public class TokenHolder : ITokenHolder
{
	private readonly ILoaderProxy loaderProxy;
	private readonly ILogger<ITokenHolder> logger;
	
	private readonly Dictionary<string, TokenEntry> entries = new Dictionary<string, TokenEntry>();

	public TokenHolder(ILoaderProxy loaderProxy, ILogger<ITokenHolder> logger)
	{
		this.loaderProxy = loaderProxy;
		this.logger = logger;
	}

	public FacebookPageToken? GetFacebookPageToken(TokenKey key)
	{
		var facebookParameters = new FacebookParameters(channel.ChannelAccountId, token.Token1);
	}
}

