using System.Collections.Concurrent;
using DataAllyEngine.Common;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;
using FacebookLoader.Content;
using FacebookLoader.Token;

namespace DataAllyEngine.Services.CreativeLoader;

public class TokenHolder : ITokenHolder
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<ITokenHolder> logger;
	
	private readonly ConcurrentDictionary<TokenKey, TokenEntry> entries = new ConcurrentDictionary<TokenKey, TokenEntry>();

	public TokenHolder(IServiceProvider serviceProvider, ILogger<ITokenHolder> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	public async Task<FacebookPageToken?> GetFacebookPageToken(TokenKey key, Channel channel)
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
		
		var facebookParameters = CreateFacebookParameters(key, channel);
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
		await ProcessFacebookPageTokens(facebookParameters, tokenAccount.Id, key.CompanyId);

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
				logger.LogInformation($"Removing expired token entry for company {entry.Value.CompanyId}, page {entry.Value.PageId}");
				entries.TryRemove(entry.Key, out _);
			}
		}

		logger.LogInformation("Expired token entries cleared");
	}

	private FacebookParameters? CreateFacebookParameters(TokenKey key, Channel channel)
	{
		using (var scope = serviceProvider.CreateScope())
		{
			var loaderProxy = scope.ServiceProvider.GetRequiredService<ILoaderProxy>();

			var token = loaderProxy.GetTokenByCompanyIdAndChannelTypeId(key.CompanyId, channel.ChannelTypeId);
			if (token == null)
			{
				logger.LogWarning($"Facebook token for company with ID {key.CompanyId} not found");
				return null;
			}

			return new FacebookParameters(channel.ChannelAccountId, token.Token1);
		}
	}

	private async Task ProcessFacebookPageTokens(FacebookParameters facebookParameters, string tokenAccountId, int companyId)
	{
		var pageTokens = await TokenFetcher.GetFacebookPageTokensForAccount(facebookParameters, tokenAccountId);
		if (pageTokens.Count == 0)
		{
			logger.LogWarning($"No Facebook page tokens found for account {tokenAccountId}");
			return;
		}
		
		foreach (var pageToken in pageTokens)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var loaderProxy = scope.ServiceProvider.GetRequiredService<ILoaderProxy>();

				var facebookPageToken = new FacebookPageToken()
				{
					PageId = pageToken.PageId,
					Name = pageToken.Name,
					Token = pageToken.Token
				};
					
				var tokenEntry = new TokenEntry(companyId, facebookPageToken, DateTime.UtcNow.AddDays(TokenEntry.EXPIRE_AFTER_DAYS));
				var tokenKey = tokenEntry.GetKey();

				entries.AddOrUpdate(tokenKey, tokenEntry, (key, entry) => tokenEntry);
			}
		}
	}
}
