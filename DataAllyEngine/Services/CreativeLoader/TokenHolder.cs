using System.Collections.Concurrent;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;
using FacebookLoader.Content;
using FacebookLoader.Token;

namespace DataAllyEngine.Services.CreativeLoader;

public class TokenHolder : ITokenHolder
{
	private readonly ILoaderProxy loaderProxy;
	private readonly ILogger<ITokenHolder> logger;
	
	private readonly ConcurrentDictionary<TokenKey, TokenEntry> entries = new ConcurrentDictionary<TokenKey, TokenEntry>();

	public TokenHolder(ILoaderProxy loaderProxy, ILogger<ITokenHolder> logger)
	{
		this.loaderProxy = loaderProxy;
		this.logger = logger;
	}

	public async Task<FacebookPageToken?> GetFacebookPageToken(TokenKey key)
	{
		if (entries.TryGetValue(key, out var entry))
		{
			if (!entry.IsExpired())
			{
				entry.LastUsedUtc = DateTime.UtcNow;
				return entry.PageToken;
			}
			entries.TryRemove(key, out _);
		}
		
		var facebookParameters = CreateFacebookParameters(key);
		if (facebookParameters == null)
		{
			logger.LogWarning($"Facebook parameters for key {key} could not be created");
			return null;
		}
		
		var tokenAccount = await TokenFetcher.GetFacebookAccountForToken(facebookParameters);
		if (tokenAccount == null)
		{
			logger.LogWarning($"Facebook token for key {key} could not be fetched");
			return null;
		}

		logger.LogInformation($"Fetching Facebook page tokens for user token account {tokenAccount.Id} controlling key {key}");
		ProcessFacebookPageTokens(facebookParameters, tokenAccount.Id, key.CompanyId);

		if (entries.TryGetValue(key, out var newEntry))
		{
			return newEntry.PageToken;
		}
		return null;
	}

	public void ClearExpiredEntries()
	{
		logger.LogInformation("Attempting to clear expired token entries");

		var now = DateTime.UtcNow;
		foreach (var entry in entries)
		{
			if (entry.Value.IsExpired())
			{
				logger.LogInformation($"Removing expired token entry for company {entry.Value.CompanyId}, channel {entry.Value.ChannelId}");
				entries.TryRemove(entry.Key, out _);
			}
		}

		logger.LogInformation("Expired token entries cleared");
	}

	private FacebookParameters? CreateFacebookParameters(TokenKey key)
	{
		var channel = loaderProxy.GetChannelById(key.ChannelId);
		if (channel == null)
		{
			logger.LogWarning($"Channel with ID {key.ChannelId} not found");
			return null;
		}
		var token = loaderProxy.GetTokenByCompanyIdAndChannelTypeId(key.CompanyId, channel.ChannelTypeId);
		if (token == null)
		{
			logger.LogWarning($"Facebook token for company with ID {key.CompanyId} not found");
			return null;
		}
		
		return new FacebookParameters(channel.ChannelAccountId, token.Token1);
	}

	private async void ProcessFacebookPageTokens(FacebookParameters facebookParameters, string tokenAccountId, int companyId)
	{
		var pageTokens = await TokenFetcher.GetFacebookPageTokensForAccount(facebookParameters, tokenAccountId);
		if (pageTokens.Count == 0)
		{
			logger.LogWarning($"No Facebook page tokens found for account {tokenAccountId}");
			return;
		}
		
		foreach (var pageToken in pageTokens)
		{
			var channel = loaderProxy.GetChannelByChannelAccountId(pageToken.Id);
			if (channel == null)
			{
				logger.LogInformation($"Channel not found for Facebook page token account {pageToken.Id} - skipping");
				continue;
			}
			var facebookPageToken = new FacebookPageToken(pageToken.Id, pageToken.Name, pageToken.Token);
			var tokenEntry = new TokenEntry(companyId, channel.Id, facebookPageToken, DateTime.UtcNow.AddDays(TokenEntry.EXPIRE_AFTER_DAYS));
			var tokenKey = tokenEntry.GetKey();

			entries.AddOrUpdate(tokenKey, tokenEntry, (key, entry) => tokenEntry);
		}
	}
}
