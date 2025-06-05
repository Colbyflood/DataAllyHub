using FacebookLoader.Loader.AdCreative;

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
	public string PageId { get; }
	public string VideoId { get; }
	public string Title { get; }
	public string Message { get; }
	public string LinkDescription { get; }
	public FacebookCallToAction CallToAction { get; }
	public string ImageUrl { get; }
	public string ImageHash { get; }
	
	public FacebookVideoData(string pageId, string videoId, string title, 
		string message, string linkDescription, 
		string imageUrl, string imageHash, FacebookCallToAction callToAction)
	{
		PageId = pageId;
		VideoId = videoId;
		Title = title;
		Message = message;
		LinkDescription = linkDescription;
		ImageUrl = imageUrl;
		ImageHash = imageHash;
		CallToAction = callToAction;
	}
}

public class FacebookChildAttachment
{
	public string Link { get; }
	public string ImageHash { get; }
	public string Name { get; }
	public FacebookCallToAction CallToAction { get; }

	public FacebookChildAttachment(string link, string imageHash, string name, FacebookCallToAction callToAction)
	{
		Link = link;
		ImageHash = imageHash;
		Name = name;
		CallToAction = callToAction;
	}
}

public class FacebookLinkData
{
	public string PageId { get; }
	public string Message { get; }
	public string ImageHash { get; }
	public FacebookCallToAction CallToAction { get; }
	public List<FacebookChildAttachment> ChildAttachments { get; }

	public FacebookLinkData(string pageId, string message, string imageHash, FacebookCallToAction callToAction, List<FacebookChildAttachment> childAttachments)
	{
		PageId = pageId;
		Message = message;
		ImageHash = imageHash;
		CallToAction = callToAction;
		ChildAttachments = childAttachments;
	}
}

public class FacebookPhotoData
{
	public string PageId { get; }
	public string ImageHash { get; }

	public FacebookPhotoData(string pageId, string imageHash)
	{
		PageId = pageId;
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
	public string ImageHash { get; }
	public string VideoId { get; }
	public string? PageId { get; }
	public FacebookVideoData VideoData { get; }
	public FacebookLinkData LinkData { get; }
	public FacebookPhotoData PhotoData { get; }
	
	public FacebookCreative(string id, string status, string actorId, 
		string instagramActorId, string instagramPermalinkUrl, string objectType, 
		string thumbnailUrl, string thumbnailId, string urlTags, string title, 
		string body, string imageHash, string videoId, string pageId, 
		FacebookVideoData videoData, FacebookLinkData linkData, FacebookPhotoData photoData)
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
		ImageHash = imageHash;
		VideoId = videoId;
		PageId = PageId;
		VideoData = videoData;
		LinkData = linkData;
		PhotoData = photoData;
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
