namespace DataAllyEngine.LoaderTask;

public class NotificationMessage
{
	public string Type { get; set; }
	public string Source { get; set; }
	public string Content { get; set; }
}


public class NotificationTypes
{
	// ReSharper disable InconsistentNaming
	public const string CREATE_FEED_TRIGGER = "CreateFeed";
	public const string PROCESS_BACKFILL_TRIGGER = "ProcessBackfill";
	public const string DATA_LOADED_TRIGGER = "DataLoaded";
	public const string RESTART_LOAD_TRIGGER = "RestartLoad";
	public const string DAILY_FACEBOOK_RUN = "DailyFacebookRun";
	public const string PROCESS_FACEBOOK_DATA_REQUEST = "ProcessFacebookDataRequest";
}
