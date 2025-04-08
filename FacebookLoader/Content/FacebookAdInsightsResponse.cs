using Newtonsoft.Json;

namespace FacebookLoader.Content;

public class FacebookAdInsightsResponse
{
	public List<FacebookAdInsight> Content { get; }
	public bool IsSuccessful { get; }
	public string? RestartUrl { get; }
	public bool NotPermitted { get; }
	public bool TokenExpired { get; }
	public bool Throttled { get; }

	[JsonConstructor]
	public FacebookAdInsightsResponse(
		List<FacebookAdInsight> content,
		bool isSuccessful = true,
		string? restartUrl = null,
		bool notPermitted = false,
		bool tokenExpired = false,
		bool throttled = false)
	{
		Content = content;
		IsSuccessful = isSuccessful;
		RestartUrl = restartUrl;
		NotPermitted = notPermitted;
		TokenExpired = tokenExpired;
		Throttled = throttled;
	}
	
	public static FacebookAdInsightsResponse? FromJson(string json)
	{
		return JsonConvert.DeserializeObject<FacebookAdInsightsResponse>(json);
	}
	
	public string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.None);
	}
}