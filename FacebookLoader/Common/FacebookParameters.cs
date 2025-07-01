namespace FacebookLoader.Common;

public class FacebookParameters
{
	public string AccountId { get; }
	public string Token { get; }
	public string FacebookVersion { get; }

	public FacebookParameters(string accountId, string token, string facebookVersion = "23.0")
	{
		AccountId = accountId;
		Token = token;
		FacebookVersion = facebookVersion;
	}

	public string CreateUrlFor(string endpoint)
	{
		var accountValue = AccountId;
		if (!accountValue.StartsWith("act_"))
		{
			accountValue = $"act_{AccountId}";
		}

		return $"https://graph.facebook.com/v{FacebookVersion}/{accountValue}/{endpoint}";
	}
	
	public string GetBaseUrl()
	{
		return $"https://graph.facebook.com/v{FacebookVersion}";
	}
}