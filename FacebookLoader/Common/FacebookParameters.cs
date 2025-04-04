namespace FacebookLoader.Common;

public class FacebookParameters
{
	public readonly string accountId;
	public readonly string token;
	public readonly string facebookVersion;

	public FacebookParameters(string accountId, string token, string facebookVersion = "22.0")
	{
		this.accountId = accountId;
		this.token = token;
		this.facebookVersion = facebookVersion;
	}

	public string CreateUrlFor(string endpoint)
	{
		var accountValue = accountId;
		if (!accountValue.StartsWith("act_"))
		{
			accountValue = $"act_{accountId}";
		}

		return $"https://graph.facebook.com/v{facebookVersion}/{accountValue}/{endpoint}";
	}
}