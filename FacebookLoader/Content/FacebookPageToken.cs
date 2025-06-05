using Newtonsoft.Json;

namespace FacebookLoader.Content;

public class FacebookPageToken
{
	[JsonProperty("id")]
	public string PageId { get; set; } = null!;
	
	[JsonProperty("name")]
	public string Name { get; set; } = null!;
	
	[JsonProperty("access_token")]
	public string Token { get; set; } = null!;
}