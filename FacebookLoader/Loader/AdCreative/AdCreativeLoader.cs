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
    private const string FIELDS_LIST = "account_id,id,name,status,adset_id,campaign_id,created_time,updated_time,creative{id,status," +
        "actor_id,instagram_actor_id,instagram_permalink_url,object_type,image_url,image_hash,thumbnail_url," +
        "thumbnail_id,product_set_id,url_tags,title,body,link_destination_display_url,product_data,template_url_spec," +
        "template_url,object_story_spec{page_id,link_data,video_data,template_data}}";

    private const int LIMIT = 30;
    private const int MAX_TEST_LOOPS = 4;

    public FacebookAdCreativesLoader(FacebookParameters facebookParameters) : base(facebookParameters) { }

    private static FacebookCallToAction DigestCallToAction(CallToAction call)
    {
        return new FacebookCallToAction(call.Type, call.Value.Link);
    }

    private static FacebookVideoData DigestVideoData(ObjectStorySpec spec)
    {
        var callToAction = DigestCallToAction(spec.VideoData.CallToAction);
        return new FacebookVideoData(
            spec.PageId,
            spec.VideoData.VideoId,
            spec.VideoData.Title,
            spec.VideoData.Message,
            spec.VideoData.LinkDescription,
            callToAction,
            spec.VideoData.ImageUrl,
            spec.VideoData.ImageHash
        );
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

    public FacebookAdCreativesResponse StartLoad(bool testMode = false)
    {
        string url = $"{FacebookParameters.CreateUrlFor("ads")}?fields={FIELDS_LIST}&limit={LIMIT}&access_token={FacebookParameters.Token}";
        return Load(url, testMode);
    }

    public async FacebookAdCreativesResponse Load(string startUrl, bool testMode = false)
    {
        int loopCount = 0;
        string currentUrl = startUrl;
        var records = new List<FacebookAdCreative>();

        while (true)
        {
            try
            {
                var data = await CallGraphApiAsync(currentUrl);
                var root = FacebookLoader.Root.FromJson(data);

                foreach (var item in root.Data)
                {
                    records.Add(DigestItem(item));
                }

                if (string.IsNullOrEmpty(root.Paging.Next) || (testMode && loopCount >= MAX_TEST_LOOPS))
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
    public FacebookLoader.Cursors Cursors { get; set; }
    public string Next { get; set; }
}

class Value
{
    public string Link { get; set; }
}

class CallToAction
{
    public Value Value { get; set; }

    public static CallToAction FromJson(JsonElement obj)
    {
        var callToAction = new CallToAction
        {
            Value = new Value
            {
                Link = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj.GetProperty("value"), "link")
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
            VideoId = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "video_id"),
            Title = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "title"),
            Message = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "message"),
            LinkDescription = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "link_description"),
            CallToAction = CallToAction.FromJson(obj.GetProperty("call_to_action")),
            ImageUrl = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "image_url"),
            ImageHash = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "image_hash")
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
            PageId = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "page_id"),
            VideoData = VideoData.FromJson(obj.GetProperty("video_data"))
        };
    }
}

class Creative
{
    public ObjectStorySpec ObjectStorySpec { get; set; }

    public static Creative FromJson(JsonElement obj)
    {
        return new Creative
        {
            ObjectStorySpec = ObjectStorySpec.FromJson(obj.GetProperty("object_story_spec"))
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
            AccountId = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "account_id"),
            Id = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "id"),
            Name = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "name"),
            Status = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "status"),
            AdsetId = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "adset_id"),
            CampaignId = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "campaign_id"),
            CreatedTime = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "created_time"),
            UpdatedTime = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj, "updated_time"),
            Creative = Creative.FromJson(obj.GetProperty("creative"))
        };
    }
}

class Root
{
    public List<Content> Data { get; set; }
    public FacebookLoader.Paging Paging { get; set; }

    public static FacebookLoader.Root FromJson(JsonElement obj)
    {
        var dataList = new List<Content>();
        foreach (var item in global::FacebookLoader.Content.FacebookLoaderBase.ExtractObjectArray(obj, "data"))
        {
            dataList.Add(Content.FromJson(item));
        }

        return new FacebookLoader.Root
        {
            Data = dataList,
            Paging = new FacebookLoader.Paging
            {
                Cursors = new FacebookLoader.Cursors
                {
                    Before = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj.GetProperty("paging").GetProperty("cursors"), "before"),
                    After = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj.GetProperty("paging").GetProperty("cursors"), "after")
                },
                Next = global::FacebookLoader.Content.FacebookLoaderBase.ExtractString(obj.GetProperty("paging"), "next")
            }
        };
    }
}


