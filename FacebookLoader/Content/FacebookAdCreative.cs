namespace FacebookLoader.Content;


public class FacebookCallToAction
{
	public string Type { get; }
	public string Link { get; }
	
	public FacebookCallToAction(string type, string link)
	{
		Type = type;
		Link = link;
	}
}

public class FacebookVideoData
{
	public static FacebookVideoData Instance { get; }
	public string PageId { get; }
	public string VideoId { get; }
	public string Title { get; }
	public string Message { get; }
	public string LinkDescription { get; }
	public FacebookCallToAction CallToAction { get; }
	public string ImageUrl { get; }
	public string ImageHash { get; }
	
	public FacebookVideoData(string pageId, string videoId, string title, 
		string message, string linkDescription, FacebookCallToAction callToAction, 
		string imageUrl, string imageHash)
	{
		PageId = pageId;
		VideoId = videoId;
		Title = title;
		Message = message;
		LinkDescription = linkDescription;
		CallToAction = callToAction;
		ImageUrl = imageUrl;
		ImageHash = imageHash;
	}
}

public class FacebookCreative
{
	public string Id { get; }
	public string Status { get; }
	public string ActorId { get; }
	public string InstagramActorId { get; }
	public string InstagramPermalinkUrl { get; }
	public string ObjectType { get; }
	public string ThumbnailUrl { get; }
	public string ThumbnailId { get; }
	public string UrlTags { get; }
	public string Title { get; }
	public string Body { get; }
	public FacebookVideoData VideoData { get; }
	
	public FacebookCreative(string id, string status, string actorId, 
		string instagramActorId, string instagramPermalinkUrl, string objectType, 
		string thumbnailUrl, string thumbnailId, string urlTags, string title, 
		string body, FacebookVideoData videoData)
	{
		Id = id;
		Status = status;
		ActorId = actorId;
		InstagramActorId = instagramActorId;
		InstagramPermalinkUrl = instagramPermalinkUrl;
		ObjectType = objectType;
		ThumbnailUrl = thumbnailUrl;
		ThumbnailId = thumbnailId;
		UrlTags = urlTags;
		Title = title;
		Body = body;
		VideoData = videoData;
	}
}

public class FacebookAdCreative
{
	public string Id { get; }
	public string Name { get; }
	public string AccountId { get; }
	public string Status { get; }
	public string AdsetId { get; }
	public string CampaignId { get; }
	public string CreatedTime { get; }
	public string UpdatedTime { get; }
	public FacebookCreative Creative { get; }
	
	public FacebookAdCreative(string id, string name, string accountId, string status, 
		string adsetId, string campaignId, string createdTime, string updatedTime, FacebookCreative creative)
	{
		Id = id;
		Name = name;
		AccountId = accountId;
		Status = status;
		AdsetId = adsetId;
		CampaignId = campaignId;
		CreatedTime = createdTime;
		UpdatedTime = updatedTime;
		Creative = creative;
	}
}
