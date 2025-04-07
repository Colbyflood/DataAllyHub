using System;
using System.Collections.Generic;
using System.Text.Json;
using FacebookLoader.Common;
using FacebookLoader.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FacebookLoader.Loader.AdCreative;


public class FacebookAdCreativesLoader : FacebookLoaderBase
{
    private const string FieldsList = "account_id,id,name,status,adset_id,campaign_id,created_time,updated_time,creative{id,status," +
        "actor_id,instagram_actor_id,instagram_permalink_url,object_type,image_url,image_hash,thumbnail_url," +
        "thumbnail_id,product_set_id,url_tags,title,body,link_destination_display_url,product_data,template_url_spec," +
        "template_url,object_story_spec{page_id,link_data,video_data,template_data}}";

    private const int Limit = 30;
    private const int MaxTestLoops = 4;

    public FacebookAdCreativesLoader(FacebookParameters facebookParameters, ILogging logger) : base(facebookParameters, logger) { }

    private static FacebookCallToAction DigestCallToAction(CallToAction call)
    {
        return new FacebookCallToAction(call.Type, call.Value.Link);
    }

    private static FacebookVideoData DigestVideoData(ObjectStorySpec spec)
    {
        var callToAction = DigestCallToAction(spec.VideoData.CallToAction);
        return FacebookVideoData.Instance;
    }

    private static FacebookCreative DigestCreative(Creative creative)
    {
        var videoData = DigestVideoData(creative.ObjectStorySpec);
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
            videoData
        );
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

    public async Task<FacebookAdCreativesResponse> StartLoad(bool testMode = false)
    {
        string url = $"{FacebookParameters.CreateUrlFor("ads")}?fields={FieldsList}&limit={Limit}&access_token={FacebookParameters.Token}";
        return await Load(url, testMode);
    }

    public async Task<FacebookAdCreativesResponse> Load(string startUrl, bool testMode = false)
    {
        int loopCount = 0;
        string currentUrl = startUrl;
        var records = new List<FacebookAdCreative>();

        while (true)
        {
            try
            {
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
            }
            catch (FacebookHttpException fe)
            {
                Console.WriteLine($"Caught FacebookHttpException: {fe.Message}");
                return new FacebookAdCreativesResponse(records, false, currentUrl, fe.NotPermitted, fe.TokenExpired, fe.Throttled);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught exception: {ex.Message}");
                return new FacebookAdCreativesResponse(records, false, currentUrl, true);
            }
        }

        return new FacebookAdCreativesResponse(records);
    }
}

class Cursors
{
    public string Before { get; set; }
    public string After { get; set; }
}

class Paging
{
    public Cursors Cursors { get; set; }
    public string Next { get; set; }
}

class Value
{
    public string Link { get; set; }
}

class CallToAction
{
    public string Type { get; set; }
    public Value Value { get; set; }

    public static CallToAction FromJson(JsonElement obj)
    {
        var callToAction = new CallToAction
        {
            Type = obj.GetProperty("type").GetString()!,
            Value = new Value
            {
                Link = FacebookLoaderBase.ExtractString(obj.GetProperty("value"), "link")
            }
        };
        return callToAction;
    }
}

class VideoData
{
    public string VideoId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string LinkDescription { get; set; }
    public CallToAction CallToAction { get; set; }
    public string ImageUrl { get; set; }
    public string ImageHash { get; set; }

    public static VideoData FromJson(JsonElement obj)
    {
        return new VideoData
        {
            VideoId = FacebookLoaderBase.ExtractString(obj, "video_id"),
            Title = FacebookLoaderBase.ExtractString(obj, "title"),
            Message = FacebookLoaderBase.ExtractString(obj, "message"),
            LinkDescription = FacebookLoaderBase.ExtractString(obj, "link_description"),
            CallToAction = CallToAction.FromJson(obj.GetProperty("call_to_action")),
            ImageUrl = FacebookLoaderBase.ExtractString(obj, "image_url"),
            ImageHash = FacebookLoaderBase.ExtractString(obj, "image_hash")
        };
    }
}

class ObjectStorySpec
{
    public string PageId { get; set; }
    public VideoData VideoData { get; set; }

    public static ObjectStorySpec FromJson(JsonElement obj)
    {
        return new ObjectStorySpec
        {
            PageId = FacebookLoaderBase.ExtractString(obj, "page_id"),
            VideoData = VideoData.FromJson(obj.GetProperty("video_data"))
        };
    }
}

class Creative
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
    public ObjectStorySpec ObjectStorySpec { get; set; }

    public static Creative FromJson(JObject obj)
    {
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
            ObjectStorySpec = ObjectStorySpec.FromJson(FacebookLoaderBase.ExtractObject(obj, "object_story_spec"))
        };
    }
}

class Content
{
    public string AccountId { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string AdsetId { get; set; }
    public string CampaignId { get; set; }
    public string CreatedTime { get; set; }
    public string UpdatedTime { get; set; }
    public Creative Creative { get; set; }

    public static Content FromJson(JsonElement obj)
    {
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
            Creative = Creative.FromJson(obj.GetProperty("creative"))
        };
    }
}

class Root
{
    public List<Content> Data { get; set; }
    public Paging Paging { get; set; }

    public static Root FromJson(JsonElement obj)
    {
        var dataList = new List<Content>();
        foreach (var item in FacebookLoaderBase.ExtractObjectArray(obj, "data"))
        {
            dataList.Add(Content.FromJson(item));
        }

        return new Root
        {
            Data = dataList,
            Paging = new Paging
            {
                Cursors = new Cursors
                {
                    Before = FacebookLoaderBase.ExtractString(obj.GetProperty("paging").GetProperty("cursors"), "before"),
                    After = FacebookLoaderBase.ExtractString(obj.GetProperty("paging").GetProperty("cursors"), "after")
                },
                Next = FacebookLoaderBase.ExtractString(obj.GetProperty("paging"), "next")
            }
        };
    }
}


