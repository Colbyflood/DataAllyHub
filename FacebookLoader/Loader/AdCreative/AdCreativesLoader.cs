using FacebookLoader.Common;
using FacebookLoader.Content;
using Newtonsoft.Json.Linq;
using System.Web;

namespace FacebookLoader.Loader.AdCreative;


public class AdCreativesLoader : FacebookLoaderBase
{
    private const string FieldsList = "account_id,id,name,status,adset_id,campaign_id,created_time,updated_time,creative{id,status," +
        "actor_id,instagram_actor_id,video_id,instagram_permalink_url,object_type,image_url,image_hash,thumbnail_url," +
        "thumbnail_id,product_set_id,url_tags,title,body,link_destination_display_url,template_url_spec," +
        "template_url,object_story_spec{page_id,link_data,video_data,template_data}}";

    private const int Limit = 100;
    private const int MaxTestLoops = 4;

    public AdCreativesLoader(FacebookParameters facebookParameters, ILogging logger) : base(facebookParameters, logger) { }

    private static FacebookCallToAction DigestCallToAction(CallToAction call)
    {
        return new FacebookCallToAction(call.Type, call.Value.Link);
    }

    private static FacebookVideoData DigestVideoData(ObjectStorySpec spec)
    {
        var data = spec.VideoData;
        var callToAction = DigestCallToAction(data.CallToAction);
        return new FacebookVideoData(spec.PageId, data.VideoId, data.Title, data.Message,
            data.LinkDescription, data.ImageUrl, data.ImageHash, callToAction);
    }

    private static FacebookLinkData DigestLinkData(ObjectStorySpec spec)
    {
        var data = spec.LinkData;
        var childAttachments = new List<FacebookChildAttachment>();
        if (data.ChildAttachments != null)
        {
            data.ChildAttachments.ForEach(attachment =>
            {
                var attachmentCallToAction = DigestCallToAction(attachment.CallToAction);
                childAttachments.Add(new FacebookChildAttachment(attachment.Link, attachment.ImageHash, attachment.Name, attachmentCallToAction));
            });
        }

        var callToAction = DigestCallToAction(data.CallToAction);
        return new FacebookLinkData(spec.PageId, data.Message, data.ImageHash, callToAction, childAttachments);
    }

    private static FacebookPhotoData DigestPhotoData(ObjectStorySpec spec)
    {
        var data = spec.PhotoData;
        return new FacebookPhotoData(spec.PageId, data.ImageHash);
    }

    private static FacebookCreative DigestCreative(Creative creative)
    {
        var videoData = DigestVideoData(creative.ObjectStorySpec);
        var linkData = DigestLinkData(creative.ObjectStorySpec);
        var photoData = DigestPhotoData(creative.ObjectStorySpec);
        var imageHash = creative.ObjectStorySpec.ImageHash;
        var videoId = creative.ObjectStorySpec.VideoId;
        var pageId = creative.ObjectStorySpec.PageId;
        return new FacebookCreative(
            creative.Id,
            creative.Status,
            creative.ActorId,
            creative.InstagramActorId,
            creative.InstagramPermalinkUrl,
            creative.ObjectType,
            creative.ThumbnailUrl,
            creative.ThumbnailId,
            creative.UrlTags,
            creative.Title,
            creative.Body,
            imageHash,
            videoId,
            pageId,
            videoData,
            linkData,
            photoData
        );
    }

    public static string DigestJsonStringItem(string jsonString)
    {
        var jsonObject = JObject.Parse(jsonString);
        var root = Root.FromJson(jsonObject);

        var records = new List<FacebookAdCreative>();
        foreach (var item in root.Data)
        {
            records.Add(DigestItem(item));
        }
        // records.Add(DigestItem(content));
        var response = new FacebookAdCreativesResponse(records, false, "Z", false, false, false);
        return response.ToJson();
        // return JsonConvert.SerializeObject(response, Formatting.None);
    }

    private static FacebookAdCreative DigestItem(Content item)
    {
        var creative = DigestCreative(item.Creative);
        return new FacebookAdCreative(
            item.Id,
            item.Name,
            item.AccountId,
            item.Status,
            item.AdsetId,
            item.CampaignId,
            item.CreatedTime,
            item.UpdatedTime,
            creative
        );
    }

    public async Task<FacebookAdCreativesResponse?> StartLoadAsync(bool testMode = false)
    {
        string url = $"{FacebookParameters.CreateUrlFor("ads")}?fields={FieldsList}&limit={Limit}&access_token={FacebookParameters.Token}";
        return await LoadAsync(url, testMode);
    }

    public async Task<FacebookAdCreativesResponse?> LoadAsync(string startUrl, bool testMode = false)
    {
        int loopCount = 0;
        string currentUrl = startUrl;
        var records = new List<FacebookAdCreative>();

        var currentLimitSize = GetLimitFromUrl(startUrl) ?? Limit;

        int serviceDownRetriesCount = 0;

        while (true)
        {
            try
            {
                currentUrl = HttpUtility.UrlDecode(currentUrl); // Removing non URL encoded characters

                var data = await CallGraphApiAsync(currentUrl);
                var root = Root.FromJson(data);

                foreach (var item in root.Data)
                {
                    records.Add(DigestItem(item));
                }

                if (string.IsNullOrEmpty(root.Paging.Next) || (testMode && loopCount >= MaxTestLoops))
                    break;

                currentUrl = root.Paging.Next;
                loopCount++;

                serviceDownRetriesCount = 0;
            }
            catch (FacebookHttpException fe)
            {
                if (fe.RequestSizeTooLarge)
                {
                    if (currentLimitSize == 1)
                    {
                        Logger.LogException(fe, $"Caught FacebookHttpException: {fe.Message} and the Limit cannot be less than 1 - marking as NotPermitted for {GetSanitizedUrl(currentUrl)}");
                        return new FacebookAdCreativesResponse(records, false, currentUrl, true, fe.TokenExpired, fe.Throttled, fe.ServiceDown, fe.ResponseBody);
                    }
                    currentLimitSize /= 2;
                    if (currentLimitSize < 0)
                    {
                        currentLimitSize = 1;
                    }
                    Logger.LogWarning($"Cutting limit size down to {currentLimitSize} for {GetSanitizedUrl(currentUrl)}");
                    currentUrl = UpdateUrlWithLimit(currentUrl, currentLimitSize);
                }
                else if (fe.ServiceDown)
                {
                    if (++serviceDownRetriesCount > 3)
                    {
                        Logger.LogWarning($"service down to retrying {serviceDownRetriesCount} for {GetSanitizedUrl(currentUrl)}");
                        return new FacebookAdCreativesResponse(records, false, currentUrl, fe.NotPermitted, fe.TokenExpired, fe.Throttled, fe.ServiceDown, fe.ResponseBody);
                    }

                    Thread.Sleep(3000);
                }
                else
                {
                    Logger.LogException(fe, $"Caught FacebookHttpException: {fe.Message}");
                    return new FacebookAdCreativesResponse(records, false, currentUrl, fe.NotPermitted, fe.TokenExpired, fe.Throttled, fe.ServiceDown, fe.ResponseBody);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Caught exception: {ex.Message}");
                return new FacebookAdCreativesResponse(records, false, currentUrl, true, false, false, false, ex.Message);
            }
        }

        return new FacebookAdCreativesResponse(records);
    }
}

class Cursors
{
    public string Before { get; set; }
    public string After { get; set; }

    public static Cursors FromJson(JToken? obj)
    {
        return new Cursors
        {
            Before = FacebookLoaderBase.ExtractString(obj, "before"),
            After = FacebookLoaderBase.ExtractString(obj, "after")
        };
    }
}

class Paging
{
    public Cursors Cursors { get; set; }
    public string Next { get; set; }

    public static Paging FromJson(JToken? obj)
    {
        return new Paging
        {
            Cursors = Cursors.FromJson(FacebookLoaderBase.ExtractObject(obj, "cursors")),
            Next = FacebookLoaderBase.ExtractString(obj, "next")
        };
    }
}

class Value
{
    public string Link { get; set; } = "";
}

class CallToAction
{
    public string Type { get; set; } = "";
    public Value Value { get; set; } = new Value();

    public static CallToAction FromJson(JToken obj)
    {
        var callToAction = new CallToAction
        {
            Type = obj["type"]!.ToString(),
            Value = new Value
            {
                Link = FacebookLoaderBase.ExtractString(obj["value"], "link")
            }
        };
        return callToAction;
    }
}

class VideoData
{
    public string VideoId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string LinkDescription { get; set; } = "";
    public CallToAction CallToAction { get; set; } = new CallToAction();
    public string ImageUrl { get; set; } = "";
    public string ImageHash { get; set; } = "";

    public static VideoData FromJson(JToken obj)
    {
        var callToActionNode = obj["call_to_action"];
        CallToAction callToAction = new CallToAction();
        if (callToActionNode != null)
        {
            callToAction = CallToAction.FromJson(callToActionNode);
        }
        return new VideoData
        {
            VideoId = FacebookLoaderBase.ExtractString(obj, "video_id"),
            Title = FacebookLoaderBase.ExtractString(obj, "title"),
            Message = FacebookLoaderBase.ExtractString(obj, "message"),
            LinkDescription = FacebookLoaderBase.ExtractString(obj, "link_description"),
            CallToAction = callToAction,
            ImageUrl = FacebookLoaderBase.ExtractString(obj, "image_url"),
            ImageHash = FacebookLoaderBase.ExtractString(obj, "image_hash")
        };
    }
}

class ChildAttachment
{
    public string Link { get; set; } = "";
    public string ImageHash { get; set; } = "";
    public string Name { get; set; } = "";
    public CallToAction CallToAction { get; set; } = new CallToAction();

    public static ChildAttachment FromJson(JToken obj)
    {
        var callToActionNode = obj["call_to_action"];
        CallToAction callToAction = new CallToAction();
        if (callToActionNode != null)
        {
            callToAction = CallToAction.FromJson(callToActionNode);
        }

        return new ChildAttachment()
        {
            CallToAction = callToAction,
            Link = FacebookLoaderBase.ExtractString(obj, "link"),
            Name = FacebookLoaderBase.ExtractString(obj, "name"),
            ImageHash = FacebookLoaderBase.ExtractString(obj, "image_hash")
        };
    }
}

class LinkData
{
    public string Message { get; set; } = "";
    public CallToAction CallToAction { get; set; } = new CallToAction();
    public List<ChildAttachment> ChildAttachments { get; set; }
    public string ImageHash { get; set; } = "";

    public static LinkData FromJson(JToken obj)
    {
        var callToActionNode = obj["call_to_action"];
        CallToAction callToAction = new CallToAction();
        if (callToActionNode != null)
        {
            callToAction = CallToAction.FromJson(callToActionNode);
        }

        List<ChildAttachment> childAttachments = new List<ChildAttachment>();
        var data = FacebookLoaderBase.ExtractObjectArray(obj, "child_attachments");
        foreach (var item in data)
        {
            if (item is JObject dataObject)
                childAttachments.Add(ChildAttachment.FromJson(dataObject));
        }

        return new LinkData
        {
            ChildAttachments = childAttachments,
            Message = FacebookLoaderBase.ExtractString(obj, "message"),
            CallToAction = callToAction,
            ImageHash = FacebookLoaderBase.ExtractString(obj, "image_hash")
        };
    }
}

class PhotoData
{
    public string ImageHash { get; set; } = "";

    public static PhotoData FromJson(JToken obj)
    {
        return new PhotoData
        {
            ImageHash = FacebookLoaderBase.ExtractString(obj, "image_hash")
        };
    }
}

class ObjectStorySpec
{
    public string PageId { get; set; } = "";
    public VideoData VideoData { get; set; } = new VideoData();
    public LinkData LinkData { get; set; } = new LinkData();
    public PhotoData PhotoData { get; set; } = new PhotoData();
    public string ImageHash { get; set; } = "";
    public string VideoId { get; set; } = "";

    public static ObjectStorySpec FromJson(JToken? obj)
    {
        var pageId = FacebookLoaderBase.ExtractString(obj, "page_id");
        var imageHash = FacebookLoaderBase.ExtractString(obj, "image_hash");
        var videoId = FacebookLoaderBase.ExtractString(obj, "video_id");
        var videoDataNode = obj["video_data"];
        VideoData videoData = new VideoData();
        if (videoDataNode != null)
        {
            videoData = VideoData.FromJson(videoDataNode);
        }
        var linkDataNode = obj["link_data"];
        LinkData linkData = new LinkData();
        if (linkDataNode != null)
        {
            linkData = LinkData.FromJson(linkDataNode);
        }
        var photoDataNode = obj["photo_data"];
        PhotoData photoData = new PhotoData();
        if (photoDataNode != null)
        {
            photoData = PhotoData.FromJson(photoDataNode);
        }
        return new ObjectStorySpec
        {
            PageId = pageId,
            ImageHash = imageHash,
            VideoId = videoId,
            VideoData = videoData,
            LinkData = linkData,
            PhotoData = photoData
        };
    }
}

class Creative
{
    public string Id { get; set; } = "";
    public string Status { get; set; } = "";
    public string ActorId { get; set; } = "";
    public string InstagramActorId { get; set; } = "";
    public string InstagramPermalinkUrl { get; set; } = "";
    public string ObjectType { get; set; } = "";
    public string ThumbnailUrl { get; set; } = "";
    public string ThumbnailId { get; set; } = "";
    public string UrlTags { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public ObjectStorySpec ObjectStorySpec { get; set; } = new ObjectStorySpec();

    public static Creative FromJson(JToken obj)
    {
        var objectStorySpecNode = FacebookLoaderBase.ExtractObject(obj, "object_story_spec");
        ObjectStorySpec objectStorySpec = new ObjectStorySpec();
        if (objectStorySpecNode != null)
        {
            objectStorySpec = ObjectStorySpec.FromJson(objectStorySpecNode);
        }

        return new Creative
        {
            Id = FacebookLoaderBase.ExtractString(obj, "id"),
            Status = FacebookLoaderBase.ExtractString(obj, "status"),
            ActorId = FacebookLoaderBase.ExtractString(obj, "actor_id"),
            InstagramActorId = FacebookLoaderBase.ExtractString(obj, "instagram_actor_id"),
            InstagramPermalinkUrl = FacebookLoaderBase.ExtractString(obj, "instagram_permalink_url"),
            ObjectType = FacebookLoaderBase.ExtractString(obj, "object_type"),
            ThumbnailUrl = FacebookLoaderBase.ExtractString(obj, "thumbnail_url"),
            ThumbnailId = FacebookLoaderBase.ExtractString(obj, "thumbnail_id"),
            UrlTags = FacebookLoaderBase.ExtractString(obj, "url_tags"),
            Title = FacebookLoaderBase.ExtractString(obj, "title"),
            Body = FacebookLoaderBase.ExtractString(obj, "body"),
            ObjectStorySpec = objectStorySpec
        };
    }
}

class Content
{
    public string AccountId { get; set; } = "";
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public string AdsetId { get; set; } = "";
    public string CampaignId { get; set; } = "";
    public string CreatedTime { get; set; } = "";
    public string UpdatedTime { get; set; } = "";
    public string VideoId { get; set; } = "";
    public string ImageHash { get; set; } = "";
    public Creative Creative { get; set; } = new Creative();

    public static Content FromJson(JToken obj)
    {
        var x = obj["creative"];
        var _Creative = Creative.FromJson(obj["creative"]);
        return new Content
        {
            AccountId = FacebookLoaderBase.ExtractString(obj, "account_id"),
            Id = FacebookLoaderBase.ExtractString(obj, "id"),
            Name = FacebookLoaderBase.ExtractString(obj, "name"),
            Status = FacebookLoaderBase.ExtractString(obj, "status"),
            AdsetId = FacebookLoaderBase.ExtractString(obj, "adset_id"),
            CampaignId = FacebookLoaderBase.ExtractString(obj, "campaign_id"),
            CreatedTime = FacebookLoaderBase.ExtractString(obj, "created_time"),
            UpdatedTime = FacebookLoaderBase.ExtractString(obj, "updated_time"),
            VideoId = FacebookLoaderBase.ExtractString(obj, "video_id"),
            ImageHash = FacebookLoaderBase.ExtractString(obj, "image_hash"),
            Creative = Creative.FromJson(obj["creative"])
        };
    }
}

class Root
{
    public List<Content> Data { get; set; }
    public Paging Paging { get; set; }

    public static Root FromJson(JToken obj)
    {
        var dataList = new List<Content>();

        var extractObjects = FacebookLoaderBase.ExtractObjectArray(obj, "data");

        foreach (var item in extractObjects)
        {
            dataList.Add(Content.FromJson(item));
        }

        return new Root
        {
            Data = dataList,
            Paging = Paging.FromJson(FacebookLoaderBase.ExtractObject(obj, "paging"))
        };
    }
}


