using System.Net.Http.Headers;
using FacebookLoader.Common;
using FacebookLoader.Content;

namespace FacebookLoader.Loader.AdInsight;


public class FacebookAdInsightsLoader : FacebookLoaderBase
{
    private const string FieldsList = "id,name,created_time,updated_time,preview_shareable_link,previews.ad_format(DESKTOP_FEED_STANDARD),";

    private const string InsightFields = "{date_start,date_stop,account_id,account_name,account_currency,attribution_setting,optimization_goal,campaign_id,campaign_name," +
        "objective,buying_type,adset_id,adset_name,ad_id,ad_name,impressions,reach,frequency,spend,clicks,cpc,ctr,cpm,conversion_rate_ranking," +
        "engagement_rate_ranking,quality_ranking,actions,action_values,cost_per_action_type,conversions,conversion_values,cost_per_conversion," +
        "cost_per_unique_click,cost_per_unique_conversion,outbound_clicks,outbound_clicks_ctr,inline_link_click_ctr,cost_per_outbound_click," +
        "video_30_sec_watched_actions,video_avg_time_watched_actions,video_p100_watched_actions,video_p25_watched_actions,video_p50_watched_actions," +
        "video_p75_watched_actions,video_p95_watched_actions,video_thruplay_watched_actions,video_play_actions,cost_per_thruplay}";

    private const int Limit = 50;
    private const int MaxTestLoops = 4;
    private readonly string _accessToken;
    private readonly string _baseUrl;

    public FacebookAdInsightsLoader(FacebookParameters facebookParameters) : base(facebookParameters) {}

    private async Task<List<Datum>> LoadAsync(string startDate, string endDate, bool testMode = false)
    {
	    int loopCount = 0;
	    string currentUrl = startUrl;
	    var records = new List<FacebookAdInsight>();

	    while (true)
	    {
		    try
		    {
			    var data = FacebookAdInsightsLoader.CallGraphApi(currentUrl);
			    var root = Root.FromDictionary(data);

			    foreach (var item in root.Data)
			    {
				    var previews = FacebookAdInsightsLoader.DigestPreviews(item.Previews);
				    var insights = FacebookAdInsightsLoader.DigestInsights(item.Insights);

				    records.Add(new FacebookAdInsight
				    {
					    Id = item.Id,
					    Name = item.Name,
					    CreatedTime = item.CreatedTime,
					    UpdatedTime = item.UpdatedTime,
					    PreviewShareableLink = item.PreviewShareableLink,
					    Previews = previews,
					    Insights = insights
				    });
			    }

			    if (string.IsNullOrEmpty(root.Paging?.Next) || (testMode && loopCount >= this.MAX_TEST_LOOPS))
			    {
				    break;
			    }

			    currentUrl = root.Paging.Next;
			    loopCount++;
		    }
		    catch (FacebookHttpException fe)
		    {
			    Logging.Info($"Caught FacebookHttpException at FacebookInsightsLoader.Load(): {fe}");
			    return new FacebookAdInsightsResponse(records, false, currentUrl, fe.NotPermitted, fe.TokenExpired, fe.Throttled);
		    }
		    catch (Exception ex)
		    {
			    Logging.Info($"Caught exception at FacebookAdInsightsLoader.Load(): {ex}");
			    return new FacebookAdInsightsResponse(records, false, currentUrl, true);
		    }
	    }

	    return new FacebookAdInsightsResponse(records);
    }
    

    private string PrepareInsights(string startDate, string endDate)
    {
        return $"insights.time_range(\"{{\\\"since\\\":\\\"{startDate}\\\",\\\"until\\\":\\\"{endDate}\\\"}}\").time_increment(1).limit(1000){InsightFields}";
    }
    
    private static List<string> DigestPreviews(Previews previews)
    {
	    var digest = new List<string>();

	    if (previews?.Data != null)
	    {
		    foreach (var preview in previews.Data)
		    {
			    if (!string.IsNullOrEmpty(preview.Body))
			    {
				    digest.Add(preview.Body);
			    }
		    }
	    }

	    return digest;
    }
    
    public static string ExtractValueFrom(List<Action> actions, string actionType)
    {
	    foreach (var action in actions)
	    {
		    if (action.ActionType == actionType)
		    {
			    return action.Value;
		    }
	    }

	    return string.Empty;
    }

    public static int? ConvertToInteger(string value)
    {
	    if (string.IsNullOrEmpty(value))
		    return null;

	    if (int.TryParse(value, out var result))
		    return result;

	    return null;
    }

    public static float? ConvertToDecimal(string value)
    {
	    if (string.IsNullOrEmpty(value))
		    return null;

	    if (float.TryParse(value, out var result))
		    return result;

	    return null;
    }

    public static ActionData ExtractActionFrom(
	    List<Action> actions,
	    List<Action> values,
	    List<Action> costs,
	    string actionType)
    {
	    var count = ExtractValueFrom(actions, actionType);
	    var value = ExtractValueFrom(values, actionType);
	    var cost = ExtractValueFrom(costs, actionType);

	    return new ActionData
	    {
		    Count = ConvertToInteger(count),
		    Value = ConvertToDecimal(value),
		    CostPerAction = ConvertToDecimal(cost)
	    };
    }
    
	public static List<FacebookInsight> DigestInsights(Insights insights)
    {
        var digest = new List<FacebookInsight>();

        if (!string.IsNullOrEmpty(insights?.Paging?.Next))
        {
            Console.Error.WriteLine("There is internal pagination in Insights that has not been acted upon.");
        }

        foreach (var insight in insights.Data)
        {
            var video_30_sec_watched = FacebookInsightUtils.ExtractValueFrom(insight.Video30SecWatchedActions, "video_view");
            var video_avg_time_watched = FacebookInsightUtils.ExtractValueFrom(insight.VideoAvgTimeWatchedActions, "video_view");
            var video_p100_watched = FacebookInsightUtils.ExtractValueFrom(insight.VideoP100WatchedActions, "video_view");
            var video_p25_watched = FacebookInsightUtils.ExtractValueFrom(insight.VideoP25WatchedActions, "video_view");
            var video_p50_watched = FacebookInsightUtils.ExtractValueFrom(insight.VideoP50WatchedActions, "video_view");
            var video_p75_watched = FacebookInsightUtils.ExtractValueFrom(insight.VideoP75WatchedActions, "video_view");
            var video_p95_watched = FacebookInsightUtils.ExtractValueFrom(insight.VideoP95WatchedActions, "video_view");
            var video_thruplay_watched = FacebookInsightUtils.ExtractValueFrom(insight.VideoThruplayWatchedActions, "video_view");
            var video_play = FacebookInsightUtils.ExtractValueFrom(insight.VideoPlayActions, "video_view");
            var cost_per_thruplay = FacebookInsightUtils.ExtractValueFrom(insight.CostPerThruplay, "video_view");

            var outbound_clicks = FacebookInsightUtils.ExtractValueFrom(insight.OutboundClicks, "outbound_click");
            var outbound_clicks_ctr = FacebookInsightUtils.ExtractValueFrom(insight.OutboundClicksCtr, "outbound_click");
            var cost_per_outbound_click = FacebookInsightUtils.ExtractValueFrom(insight.CostPerOutboundClick, "outbound_click");

            var add_payment = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_payment_info");
            var add_to_cart = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_to_cart");
            var add_to_wishlist = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_to_wishlist");
            var mobile_add_payment = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_add_payment_info");
            var mobile_add_to_cart = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_add_to_cart");
            var mobile_add_to_wishlist = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_add_to_wishlist");
            var mobile_app_install = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_install");
            var mobile_complete_registration = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_complete_registration");
            var mobile_initiated_checkout = FacebookInsightUtils.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_initiated_checkout");
			var appInstall = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_install");
			var appUse = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_use");
			var comment = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "comment");
			var completeRegistration = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_complete_registration");
			var contactMobileApp = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "contact_mobile_app");
			var contactOffline = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "contact_offline");
			var contactTotal = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "contact_total");
			var contactWebsite = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "contact_website");
			var findLocationMobile = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "find_location_mobile_app");
			var findLocationOffline = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "find_location_offline");
			var findLocationTotal = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "find_location_total");
			var findLocationWebsite = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "find_location_website");
			var initiateCheckout = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_initiate_checkout");
			var landingPageView = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "landing_page_view");
			var lead = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "lead");
			var like = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "like");
			var linkClick = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "link_click");
			var offsiteConversionAddPayment = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_payment_info");
			var offsiteConversionAddToCart = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_payment_info");
			var offsiteConversionAddToWishlist = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_to_wishlist");
			var offsiteConversionCompleteRegistration = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_complete_registration");
			var offsiteConversionInitiateCheckout = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_initiate_checkout");
			var offsiteConversionLead = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_lead");
			var offsiteConversionPurchase = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_purchase");
			var offsiteConversionViewContent = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_view_content");
			var omniPurchase = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "omni_purchase");
			var onsiteConversionPostSave = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_conversion.post_save");
			var onsiteConversionPurchase = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_conversion.purchase");
			var pageEngagement = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "page_engagement");
			var photoView = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "photo_view");
			var post = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "post");
			var postEngagement = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "post_engagement");
			var postReaction = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "post_reaction");
			var scheduleMobileApp = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "schedule_mobile_app");
			var scheduleOffline = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "schedule_offline");
			var scheduleTotal = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "schedule_total");
			var scheduleWebsite = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "schedule_website");
			var startTrialMobileApp = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "start_trial_mobile_app");
			var startTrialOffline = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "start_trial_offline");
			var startTrialTotal = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "start_trial_total");
			var startTrialWebsite = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "start_trial_website");
			var submitApplicationMobileApp = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "submit_application_mobile_app");
			var submitApplicationOffline = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "submit_application_offline");
			var submitApplicationTotal = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "submit_application_total");
			var submitApplicationWebsite = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "submit_application_website");
			var subscribeMobileApp = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "subscribe_mobile_app");
			var subscribeTotal = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "subscribe_total");
			var subscribeWebsite = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "subscribe_website");
			var videoView = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "video_view");
			var onsitePurchases = FacebookAdInsightsLoader.ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_conversion.purchase");


            var insightModel = new FacebookInsight
            {
                DateStart = insight.DateStart,
                DateStop = insight.DateStop,
                AccountId = insight.AccountId,
                AccountName = insight.AccountName,
                AccountCurrency = insight.AccountCurrency,
                AttributionSetting = insight.AttributionSetting,
                OptimizationGoal = insight.OptimizationGoal,
                CampaignId = insight.CampaignId,
                CampaignName = insight.CampaignName,
                Objective = insight.Objective,
                BuyingType = insight.BuyingType,
                AdsetId = insight.AdsetId,
                AdsetName = insight.AdsetName,
                AdId = insight.AdId,
                AdName = insight.AdName,
                Impressions = FacebookInsightUtils.ConvertToInteger(insight.Impressions),
                Reach = insight.Reach,
                Frequency = insight.Frequency,
                Spend = FacebookInsightUtils.ConvertToDecimal(insight.Spend),
                Clicks = FacebookInsightUtils.ConvertToInteger(insight.Clicks),
                Cpc = FacebookInsightUtils.ConvertToDecimal(insight.Cpc),
                Ctr = FacebookInsightUtils.ConvertToDecimal(insight.Ctr),
                Cpm = FacebookInsightUtils.ConvertToDecimal(insight.Cpm),
                ConversionRateRanking = insight.ConversionRateRanking,
                EngagementRateRanking = insight.EngagementRateRanking,
                QualityRanking = insight.QualityRanking,
                Video30SecWatched = FacebookInsightUtils.ConvertToInteger(video_30_sec_watched),
                VideoAvgTimeWatched = FacebookInsightUtils.ConvertToDecimal(video_avg_time_watched),
                VideoP100Watched = FacebookInsightUtils.ConvertToInteger(video_p100_watched),
                VideoP25Watched = FacebookInsightUtils.ConvertToInteger(video_p25_watched),
                VideoP50Watched = FacebookInsightUtils.ConvertToInteger(video_p50_watched),
                VideoP75Watched = FacebookInsightUtils.ConvertToInteger(video_p75_watched),
                VideoP95Watched = FacebookInsightUtils.ConvertToInteger(video_p95_watched),
                VideoThruplayWatched = FacebookInsightUtils.ConvertToInteger(video_thruplay_watched),
                VideoPlay = FacebookInsightUtils.ConvertToInteger(video_play),
                CostPerThruplay = FacebookInsightUtils.ConvertToDecimal(cost_per_thruplay),
                CostPerUniqueClick = FacebookInsightUtils.ConvertToInteger(insight.CostPerUniqueClick),
                OutboundClicks = FacebookInsightUtils.ConvertToInteger(outbound_clicks),
                OutboundClicksCtr = FacebookInsightUtils.ConvertToDecimal(outbound_clicks_ctr),
                CostPerOutboundClick = FacebookInsightUtils.ConvertToDecimal(cost_per_outbound_click),
                InlineLinkClickCtr = FacebookInsightUtils.ConvertToDecimal(insight.InlineLinkClickCtr),
                AddPayment = add_payment,
                AddToCart = add_to_cart,
                AddToWishlist = add_to_wishlist,
                MobileAddPayment = mobile_add_payment,
                MobileAddToCart = mobile_add_to_cart,
                MobileAddToWishlist = mobile_add_to_wishlist,
                MobileAppInstall = mobile_app_install,
                MobileCompleteRegistration = mobile_complete_registration,
                MobileInitiatedCheckout = mobile_initiated_checkout,
                MobilePurchase = mobile_purchase
                AppInstall = appInstall,
			    AppUse = appUse,
			    Comment = comment,
			    CompleteRegistration = completeRegistration,
			    ContactMobileApp = contactMobileApp,
			    ContactOffline = contactOffline,
			    ContactTotal = contactTotal,
			    ContactWebsite = contactWebsite,
			    FindLocationMobile = findLocationMobile,
			    FindLocationOffline = findLocationOffline,
			    FindLocationTotal = findLocationTotal,
			    FindLocationWebsite = findLocationWebsite,
			    InitiateCheckout = initiateCheckout,
			    LandingPageView = landingPageView,
			    Lead = lead,
			    Like = like,
			    LinkClick = linkClick,
			    OffsiteConversionAddPayment = offsiteConversionAddPayment,
			    OffsiteConversionAddToCart = offsiteConversionAddToCart,
			    OffsiteConversionAddToWishlist = offsiteConversionAddToWishlist,
			    OffsiteConversionCompleteRegistration = offsiteConversionCompleteRegistration,
			    OffsiteConversionInitiateCheckout = offsiteConversionInitiateCheckout,
			    OffsiteConversionLead = offsiteConversionLead,
			    OffsiteConversionPurchase = offsiteConversionPurchase,
			    OffsiteConversionViewContent = offsiteConversionViewContent,
			    OmniPurchase = omniPurchase,
			    OnsiteConversionPostSave = onsiteConversionPostSave,
			    OnsiteConversionPurchase = onsiteConversionPurchase,
			    PageEngagement = pageEngagement,
			    PhotoView = photoView,
			    Post = post,
			    PostEngagement = postEngagement,
			    PostReaction = postReaction,
			    ScheduleMobileApp = scheduleMobileApp,
			    ScheduleOffline = scheduleOffline,
			    ScheduleTotal = scheduleTotal,
			    ScheduleWebsite = scheduleWebsite,
			    StartTrialMobileApp = startTrialMobileApp,
			    StartTrialOffline = startTrialOffline,
			    StartTrialTotal = startTrialTotal,
			    StartTrialWebsite = startTrialWebsite,
			    SubmitApplicationMobileApp = submitApplicationMobileApp,
			    SubmitApplicationOffline = submitApplicationOffline,
			    SubmitApplicationTotal = submitApplicationTotal,
			    SubmitApplicationWebsite = submitApplicationWebsite,
			    SubscribeMobileApp = subscribeMobileApp,
			    SubscribeTotal = subscribeTotal,
			    SubscribeWebsite = subscribeWebsite,
			    VideoView = videoView,
			    OnsitePurchases = onsitePurchases
            };

            digest.Add(insightModel);
        }

        return digest;
    }
}

class Action
{
	public string ActionType { get; set; }
	public string ActionDevice { get; set; }
	public string Value { get; set; }
}


class PreviewDatum
{
	public string Body { get; set; }
}

class Previews
{
	public List<PreviewDatum> Data { get; set; }
}


class InsightDatum
{
    public string DateStart { get; set; }
    public string DateStop { get; set; }
    public string AccountId { get; set; }
    public string AccountName { get; set; }
    public string AccountCurrency { get; set; }
    public string AttributionSetting { get; set; }
    public string OptimizationGoal { get; set; }
    public string CampaignId { get; set; }
    public string CampaignName { get; set; }
    public string Objective { get; set; }
    public string BuyingType { get; set; }
    public string AdsetId { get; set; }
    public string AdsetName { get; set; }
    public string AdId { get; set; }
    public string AdName { get; set; }
    public string Impressions { get; set; }
    public string Reach { get; set; }
    public string Frequency { get; set; }
    public string Spend { get; set; }
    public string Clicks { get; set; }
    public string Cpc { get; set; }
    public string Ctr { get; set; }
    public string Cpm { get; set; }
    public string ConversionRateRanking { get; set; }
    public string EngagementRateRanking { get; set; }
    public string QualityRanking { get; set; }

    public List<Action> Actions { get; set; }
    public List<Action> ActionValues { get; set; }
    public List<Action> CostPerActionType { get; set; }

    public List<Action> Video30SecWatchedActions { get; set; }
    public List<Action> VideoAvgTimeWatchedActions { get; set; }
    public List<Action> VideoP100WatchedActions { get; set; }
    public List<Action> VideoP25WatchedActions { get; set; }
    public List<Action> VideoP50WatchedActions { get; set; }
    public List<Action> VideoP75WatchedActions { get; set; }
    public List<Action> VideoP95WatchedActions { get; set; }
    public List<Action> VideoThruplayWatchedActions { get; set; }
    public List<Action> VideoPlayActions { get; set; }

    public List<Action> CostPerThruplay { get; set; }
    public List<Action> Conversions { get; set; }
    public List<Action> ConversionValues { get; set; }
    public List<Action> CostPerConversion { get; set; }

    public string CostPerUniqueClick { get; set; }
    public List<Action> CostPerUniqueConversion { get; set; }

    public List<Action> OutboundClicks { get; set; }
    public List<Action> OutboundClicksCtr { get; set; }
    public List<Action> CostPerOutboundClick { get; set; }

    public string InlineLinkClickCtr { get; set; }
    
    // get the rest
}


class Datum
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string CreatedTime { get; set; }
	public string UpdatedTime { get; set; }
	public string PreviewShareableLink { get; set; }
	public Previews Previews { get; set; }
	public Insights Insights { get; set; }
}


class Insights
{
	public List<InsightDatum> Data { get; set; }
	public Paging Paging { get; set; }
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

class Root
{
	public List<Datum> Data { get; set; }
	public Paging Paging { get; set; }
}





