namespace FacebookLoader.Content;


public class FacebookCallToAction
{
	public string Type { get; set; }
	public string Link { get; set; }
}

public class FacebookVideoData
{
	public string PageId { get; set; }
	public string VideoId { get; set; }
	public string Title { get; set; }
	public string Message { get; set; }
	public string LinkDescription { get; set; }
	public FacebookCallToAction CallToAction { get; set; }
	public string ImageUrl { get; set; }
	public string ImageHash { get; set; }
}

public class FacebookCreative
{
	public string Id { get; set; }
	public string Status { get; set; }
	public string ActorId { get; set; }
	public string InstagramActorId { get; set; }
	public string InstagramPermalinkUrl { get; set; }
	public string ObjectType { get; set; }
	public string ThumbnailUrl { get; set; }
	public string ThumbnailId { get; set; }
	public string UrlTags { get; set; }
	public string Title { get; set; }
	public string Body { get; set; }
	public FacebookVideoData VideoData { get; set; }
}

public class FacebookAdCreative
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string AccountId { get; set; }
	public string Status { get; set; }
	public string AdsetId { get; set; }
	public string CampaignId { get; set; }
	public string CreatedTime { get; set; }
	public string UpdatedTime { get; set; }
	public FacebookCreative Creative { get; set; }
}
