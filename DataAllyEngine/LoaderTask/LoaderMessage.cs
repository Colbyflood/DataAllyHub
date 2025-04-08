

using Newtonsoft.Json;

namespace DataAllyEngine.LoaderTask;

public class LoaderMessage
{
	[JsonProperty("channel_id")]
	public string ChannelId { get; set; }

	[JsonProperty("run_time_utc")] 
	public int RunTimeUtc { get; set; } = 0;
}