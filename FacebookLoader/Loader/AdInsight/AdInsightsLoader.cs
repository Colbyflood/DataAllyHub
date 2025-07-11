using FacebookLoader.Common;
using FacebookLoader.Content;
using Newtonsoft.Json.Linq;
using System.Web;

namespace FacebookLoader.Loader.AdInsight;


public class AdInsightsLoader : FacebookLoaderBase
{
    private const string FieldsList = "id,name,created_time,updated_time,preview_shareable_link,previews.ad_format(DESKTOP_FEED_STANDARD),";

    private const string InsightFields = "{date_start,date_stop,account_id,account_name,account_currency,attribution_setting,optimization_goal,campaign_id,campaign_name," +
        "objective,buying_type,adset_id,adset_name,ad_id,ad_name,impressions,reach,frequency,spend,clicks,cpc,ctr,cpm,conversion_rate_ranking," +
        "engagement_rate_ranking,quality_ranking,actions,action_values,cost_per_action_type,conversions,conversion_values,cost_per_conversion," +
        "cost_per_unique_click,cost_per_unique_conversion,outbound_clicks,outbound_clicks_ctr,inline_link_click_ctr,cost_per_outbound_click," +
        "video_30_sec_watched_actions,video_avg_time_watched_actions,video_p100_watched_actions,video_p25_watched_actions,video_p50_watched_actions," +
        "video_p75_watched_actions,video_p95_watched_actions,video_thruplay_watched_actions,video_play_actions,cost_per_thruplay, video_continuous_2_sec_watched_actions}";

    private const int Limit = 100;
    private const int SubLimit = 500;
    private const int MaxTestLoops = 4;

    public AdInsightsLoader(FacebookParameters facebookParameters, ILogging logger) : base(facebookParameters, logger) { }

    public async Task<FacebookAdInsightsResponse?> StartLoadAsync(string startDate, string endDate, bool testMode = false)
    {
        var url =
            $"{FacebookParameters.CreateUrlFor("ads")}?fields={FieldsList}{PrepareInsights(startDate, endDate)}&limit={Limit}&access_token={FacebookParameters.Token}";

        return await LoadAsync(url, testMode);
    }

    public async Task<FacebookAdInsightsResponse?> LoadAsync(string startUrl, bool testMode = false)
    {
        var loopCount = 0;
        var currentUrl = startUrl;
        var records = new List<FacebookAdInsight>();

        int serviceDownRetriesCount = 0;

        var currentLimitSize = GetLimitFromUrl(startUrl) ?? Limit;

        while (true)
        {
            try
            {
                currentUrl = HttpUtility.UrlDecode(currentUrl); // Removing non URL encoded characters

                var data = await CallGraphApiAsync(currentUrl);
                var root = Root.FromJson(data);

                foreach (var item in root.Data)
                {
                    var previews = DigestPreviews(item.Previews);
                    var insights = DigestInsights(item.Insights);

                    records.Add(new FacebookAdInsight(
                        item.Id,
                        item.Name,
                        item.CreatedTime,
                        item.UpdatedTime,
                        item.PreviewShareableLink,
                        previews,
                        insights));
                }

                if (string.IsNullOrEmpty(root.Paging?.Next) || (testMode && loopCount >= MaxTestLoops))
                {
                    break;
                }

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
                        return new FacebookAdInsightsResponse(records, false, currentUrl, true, fe.TokenExpired, fe.Throttled, fe.ServiceDown, fe.ResponseBody);
                    }
                    currentLimitSize /= 2;
                    if (currentLimitSize < 0)
                    {
                        currentLimitSize = 1;
                    }
                    currentUrl = UpdateUrlWithLimit(currentUrl, currentLimitSize);
                }
                else if (fe.ServiceDown)
                {
                    if (++serviceDownRetriesCount > 3)
                    {
                        Logger.LogWarning($"service down to retrying {serviceDownRetriesCount} for {GetSanitizedUrl(currentUrl)}");
                        return new FacebookAdInsightsResponse(records, false, currentUrl, fe.NotPermitted, fe.TokenExpired, fe.Throttled, fe.ServiceDown, fe.ResponseBody);
                    }

                    Thread.Sleep(3000);
                }
                else
                {
                    Logger.LogInformation($"Caught FacebookHttpException at FacebookInsightsLoader.Load(): {fe}");
                    return new FacebookAdInsightsResponse(records, false, currentUrl, fe.NotPermitted, fe.TokenExpired, fe.Throttled, fe.ServiceDown, fe.ResponseBody);
                }
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Caught exception at FacebookAdInsightsLoader.Load(): {ex}");
                return new FacebookAdInsightsResponse(records, false, currentUrl, true, false, false, false, ex.Message);
            }
        }

        return new FacebookAdInsightsResponse(records);
    }


    private string PrepareInsights(string startDate, string endDate)
    {
        return $"insights.time_range({{\"since\":\"{startDate}\",\"until\":\"{endDate}\"}}).time_increment(1).limit({SubLimit}){InsightFields}";
    }

    private List<string> DigestPreviews(Previews previews)
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

    private static string ExtractValueFrom(List<Action> actions, string actionType)
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

    private static int? ConvertToInteger(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (int.TryParse(value, out var result))
            return result;

        return null;
    }

    private static float? ConvertToDecimal(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (float.TryParse(value, out var result))
            return result;

        return null;
    }

    private static ActionData ExtractActionFrom(
        List<Action> actions,
        List<Action> values,
        List<Action> costs,
        string actionType)
    {
        var count = ExtractValueFrom(actions, actionType);
        var value = ExtractValueFrom(values, actionType);
        var cost = ExtractValueFrom(costs, actionType);

        return new ActionData(ConvertToInteger(count), ConvertToDecimal(value), ConvertToDecimal(cost));
    }

    private List<FacebookInsight> DigestInsights(Insights insights)
    {
        var digest = new List<FacebookInsight>();

        if (!string.IsNullOrEmpty(insights?.Paging?.Next))
        {
            Logger.LogWarning("There is internal pagination in Insights that has not been acted upon.");
        }

        foreach (var insight in insights.Data)
        {
            var video30SecWatched = ExtractValueFrom(insight.Video30SecWatchedActions, "video_view");
            var videoAvgTimeWatched = ExtractValueFrom(insight.VideoAvgTimeWatchedActions, "video_view");
            var videoP100Watched = ExtractValueFrom(insight.VideoP100WatchedActions, "video_view");
            var videoP25Watched = ExtractValueFrom(insight.VideoP25WatchedActions, "video_view");
            var videoP50Watched = ExtractValueFrom(insight.VideoP50WatchedActions, "video_view");
            var videoP75Watched = ExtractValueFrom(insight.VideoP75WatchedActions, "video_view");
            var videoP95Watched = ExtractValueFrom(insight.VideoP95WatchedActions, "video_view");
            var videoContinuous2SecWatched = ExtractValueFrom(insight.VideoContinuous2SecWatchedActions, "video_view");
            var videoThruplayWatched = ExtractValueFrom(insight.VideoThruplayWatchedActions, "video_view");
            var videoPlay = ExtractValueFrom(insight.VideoPlayActions, "video_view");
            var costPerThruplay = ExtractValueFrom(insight.CostPerThruplay, "video_view");

            var outboundClicks = ExtractValueFrom(insight.OutboundClicks, "outbound_click");
            var outboundClicksCtr = ExtractValueFrom(insight.OutboundClicksCtr, "outbound_click");
            var costPerOutboundClick = ExtractValueFrom(insight.CostPerOutboundClick, "outbound_click");

            var addPayment = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_payment_info");
            var addToCart = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_conversion.add_to_cart");
            var addToWishlist = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_add_to_wishlist");
            var mobileAddPayment = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_add_payment_info");
            var mobileAddToCart = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_add_to_cart");
            var mobileAddToWishlist = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_add_to_wishlist");
            var mobileAppInstall = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_install");
            var mobileCompleteRegistration = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_complete_registration");
            var mobileInitiatedCheckout = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_initiated_checkout");
            var mobilePurchase = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_custom_event.fb_mobile_purchase");
            var appInstall = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_install");
            var appUse = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "app_use");
            var comment = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "comment");
            var completeRegistration = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_complete_registration");
            var contactMobileApp = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "contact_mobile_app");
            var contactOffline = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "contact_offline");
            var contactTotal = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "contact_total");
            var contactWebsite = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "contact_website");
            var findLocationMobile = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "find_location_mobile_app");
            var findLocationOffline = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "find_location_offline");
            var findLocationTotal = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "find_location_total");
            var findLocationWebsite = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "find_location_website");
            var initiateCheckout = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_conversion.initiate_checkout");
            var landingPageView = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "landing_page_view");
            var lead = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "lead");
            var like = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "like");
            var linkClick = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "link_click");
            var offsiteConversionAddPayment = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_payment_info");
            var offsiteConversionAddToCart = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_to_cart");
            var offsiteConversionAddToWishlist = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_add_to_wishlist");
            var offsiteConversionCompleteRegistration = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_complete_registration");
            var offsiteConversionInitiateCheckout = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_initiate_checkout");
            var offsiteConversionLead = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_lead");
            var offsiteConversionPurchase = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_purchase");
            var offsiteConversionViewContent = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offsite_conversion.fb_pixel_view_content");
            var omniPurchase = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "omni_purchase");
            var onsiteConversionPostSave = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_conversion.post_save");
            var onsiteConversionPurchase = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_conversion.purchase");
            var pageEngagement = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "page_engagement");
            var photoView = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "photo_view");
            var post = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "post");
            var postEngagement = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "post_engagement");
            var postReaction = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "post_reaction");
            var scheduleMobileApp = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "schedule_mobile_app");
            var scheduleOffline = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "schedule_offline");
            var scheduleTotal = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "schedule_total");
            var scheduleWebsite = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "schedule_website");
            var startTrialMobileApp = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "start_trial_mobile_app");
            var startTrialOffline = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "offline_conversion.fb_offline_trial_started");
            var startTrialTotal = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "start_trial_total");
            var startTrialWebsite = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "start_trial_website");
            var submitApplicationMobileApp = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "submit_application_mobile_app");
            var submitApplicationOffline = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "submit_application_offline");
            var submitApplicationTotal = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "submit_application_total");
            var submitApplicationWebsite = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "submit_application_website");
            var subscribeMobileApp = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "subscribe_mobile_app");
            var subscribeTotal = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "subscribe_total");
            var subscribeWebsite = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "subscribe_website");
            var videoView = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "video_view");
            var onsitePurchases = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "onsite_conversion.purchase");

            var totalAddPaymentInfo = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "add_payment_info");
            var totalAddToCart = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "omni_add_to_cart");
            var totalAddToWishlist = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "add_to_wishlist");
            var totalCheckoutInitiated = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "omni_initiated_checkout");
            var totalPurchases = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "omni_purchase");
            var leadgen = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "leadgen");
            var estimatedAdRecallers = ExtractActionFrom(insight.Actions, insight.ActionValues, insight.CostPerActionType, "estimated_ad_recallers");

            var insightModel = new FacebookInsight(
                insight.DateStart,
                insight.DateStop,
                insight.AccountId,
                insight.AccountName,
                insight.AccountCurrency,
                insight.AttributionSetting,
                insight.OptimizationGoal,
                insight.CampaignId,
                insight.CampaignName,
                insight.Objective,
                insight.BuyingType,
                insight.AdsetId,
                insight.AdsetName,
                insight.AdId,
                insight.AdName,
                insight.Reach,
                insight.Frequency,
                ConvertToDecimal(insight.Spend),
                ConvertToInteger(insight.Clicks),
                ConvertToDecimal(insight.Cpc),
                ConvertToDecimal(insight.Ctr),
                ConvertToDecimal(insight.Cpm),
                ConvertToInteger(insight.Impressions),
                insight.ConversionRateRanking,
                insight.EngagementRateRanking,
                insight.QualityRanking,
                ConvertToInteger(video30SecWatched),
                ConvertToDecimal(videoAvgTimeWatched),
                ConvertToInteger(videoP100Watched),
                ConvertToInteger(videoP25Watched),
                ConvertToInteger(videoP50Watched),
                ConvertToInteger(videoP75Watched),
                ConvertToInteger(videoP95Watched),
                ConvertToInteger(videoContinuous2SecWatched),
                ConvertToInteger(videoThruplayWatched),
                ConvertToInteger(videoPlay),
                ConvertToDecimal(costPerThruplay),
                ConvertToInteger(insight.CostPerUniqueClick),
                ConvertToInteger(outboundClicks),
                ConvertToDecimal(outboundClicksCtr),
                ConvertToDecimal(costPerOutboundClick),
                ConvertToDecimal(insight.InlineLinkClickCtr),
                addPayment,
                addToCart,
                addToWishlist,
                mobileAddPayment,
                mobileAddToCart,
                mobileAddToWishlist,
                mobileAppInstall,
                mobileCompleteRegistration,
                mobileInitiatedCheckout,
                mobilePurchase,
                appInstall,
                appUse,
                comment,
                completeRegistration,
                contactMobileApp,
                contactOffline,
                contactTotal,
                contactWebsite,
                findLocationMobile,
                findLocationOffline,
                findLocationTotal,
                findLocationWebsite,
                initiateCheckout,
                landingPageView,
                lead,
                like,
                linkClick,
                offsiteConversionAddPayment,
                offsiteConversionAddToCart,
                offsiteConversionAddToWishlist,
                offsiteConversionCompleteRegistration,
                offsiteConversionInitiateCheckout,
                offsiteConversionLead,
                offsiteConversionPurchase,
                offsiteConversionViewContent,
                omniPurchase,
                onsiteConversionPostSave,
                onsiteConversionPurchase,
                pageEngagement,
                photoView,
                post,
                postEngagement,
                postReaction,
                scheduleMobileApp,
                scheduleOffline,
                scheduleTotal,
                scheduleWebsite,
                startTrialMobileApp,
                startTrialOffline,
                startTrialTotal,
                startTrialWebsite,
                submitApplicationMobileApp,
                submitApplicationOffline,
                submitApplicationTotal,
                submitApplicationWebsite,
                subscribeMobileApp,
                subscribeTotal,
                subscribeWebsite,
                videoView,
                onsitePurchases,
                totalAddPaymentInfo,
                totalAddToCart,
                totalAddToWishlist,
                totalCheckoutInitiated,
                totalPurchases,
                leadgen,
                estimatedAdRecallers
                );


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

    public static Action FromJson(JToken? obj)
    {
        return new Action
        {
            ActionType = FacebookLoaderBase.ExtractString(obj, "action_type"),
            ActionDevice = FacebookLoaderBase.ExtractString(obj, "action_device"),
            Value = FacebookLoaderBase.ExtractString(obj, "value")
        };
    }
}


class PreviewDatum
{
    public string Body { get; set; }

    public static PreviewDatum FromJson(JToken? obj)
    {
        return new PreviewDatum
        {
            Body = FacebookLoaderBase.ExtractString(obj, "body")
        };
    }
}

class Previews
{
    public List<PreviewDatum> Data { get; set; }

    public static Previews FromJson(JToken? obj)
    {
        var list = new List<PreviewDatum>();
        var data = FacebookLoaderBase.ExtractObjectArray(obj, "data");

        foreach (var item in data)
        {
            if (item is JObject dataObject)
                list.Add(PreviewDatum.FromJson(dataObject));
        }

        return new Previews
        {
            Data = list
        };
    }
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
    public List<Action> VideoContinuous2SecWatchedActions { get; set; }
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


    private static List<Action> ExtractActionListFrom(JToken obj, string actionTag)
    {
        return FacebookLoaderBase.ExtractObjectArray(obj, actionTag)
                    .Select(item => Action.FromJson(item)).ToList();
    }

    public static InsightDatum FromJson(JToken? obj)
    {
        return new InsightDatum
        {
            DateStart = FacebookLoaderBase.ExtractString(obj, "date_start"),
            DateStop = FacebookLoaderBase.ExtractString(obj, "date_stop"),
            AccountId = FacebookLoaderBase.ExtractString(obj, "account_id"),
            AccountName = FacebookLoaderBase.ExtractString(obj, "account_name"),
            AccountCurrency = FacebookLoaderBase.ExtractString(obj, "account_currency"),
            AttributionSetting = FacebookLoaderBase.ExtractString(obj, "attribution_setting"),
            OptimizationGoal = FacebookLoaderBase.ExtractString(obj, "optimization_goal"),
            CampaignId = FacebookLoaderBase.ExtractString(obj, "campaign_id"),
            CampaignName = FacebookLoaderBase.ExtractString(obj, "campaign_name"),
            Objective = FacebookLoaderBase.ExtractString(obj, "objective"),
            BuyingType = FacebookLoaderBase.ExtractString(obj, "buying_type"),
            AdsetId = FacebookLoaderBase.ExtractString(obj, "adset_id"),
            AdsetName = FacebookLoaderBase.ExtractString(obj, "adset_name"),
            AdId = FacebookLoaderBase.ExtractString(obj, "ad_id"),
            AdName = FacebookLoaderBase.ExtractString(obj, "ad_name"),
            Impressions = FacebookLoaderBase.ExtractString(obj, "impressions"),
            Reach = FacebookLoaderBase.ExtractString(obj, "reach"),
            Frequency = FacebookLoaderBase.ExtractString(obj, "frequency"),
            Spend = FacebookLoaderBase.ExtractString(obj, "spend"),
            Clicks = FacebookLoaderBase.ExtractString(obj, "clicks"),
            Cpc = FacebookLoaderBase.ExtractString(obj, "cpc"),
            Ctr = FacebookLoaderBase.ExtractString(obj, "ctr"),
            Cpm = FacebookLoaderBase.ExtractString(obj, "cpm"),
            ConversionRateRanking = FacebookLoaderBase.ExtractString(obj, "conversion_rate_ranking"),
            EngagementRateRanking = FacebookLoaderBase.ExtractString(obj, "engagement_rate_ranking"),
            QualityRanking = FacebookLoaderBase.ExtractString(obj, "quality_ranking"),
            Actions = ExtractActionListFrom(obj, "actions"),
            ActionValues = ExtractActionListFrom(obj, "action_values"),
            CostPerActionType = ExtractActionListFrom(obj, "cost_per_action_type"),
            Video30SecWatchedActions = ExtractActionListFrom(obj, "video_30_sec_watched_actions"),
            VideoAvgTimeWatchedActions = ExtractActionListFrom(obj, "video_avg_time_watched_actions"),
            VideoP100WatchedActions = ExtractActionListFrom(obj, "video_p100_watched_actions"),
            VideoP25WatchedActions = ExtractActionListFrom(obj, "video_p25_watched_actions"),
            VideoP50WatchedActions = ExtractActionListFrom(obj, "video_p50_watched_actions"),
            VideoP75WatchedActions = ExtractActionListFrom(obj, "video_p75_watched_actions"),
            VideoP95WatchedActions = ExtractActionListFrom(obj, "video_p95_watched_actions"),
            VideoThruplayWatchedActions = ExtractActionListFrom(obj, "video_thruplay_watched_actions"),
            VideoPlayActions = ExtractActionListFrom(obj, "video_play_actions"),
            VideoContinuous2SecWatchedActions = ExtractActionListFrom(obj, "video_continuous_2_sec_watched_actions"),
            CostPerThruplay = ExtractActionListFrom(obj, "cost_per_thruplay"),
            Conversions = ExtractActionListFrom(obj, "conversions"),
            ConversionValues = ExtractActionListFrom(obj, "conversion_values"),
            CostPerConversion = ExtractActionListFrom(obj, "cost_per_conversion"),
            CostPerUniqueClick = FacebookLoaderBase.ExtractString(obj, "cost_per_unique_click"),
            CostPerUniqueConversion = ExtractActionListFrom(obj, "cost_per_unique_conversion"),
            OutboundClicks = ExtractActionListFrom(obj, "outbound_clicks"),
            OutboundClicksCtr = ExtractActionListFrom(obj, "outbound_clicks_ctr"),
            CostPerOutboundClick = ExtractActionListFrom(obj, "cost_per_outbound_click"),
            InlineLinkClickCtr = FacebookLoaderBase.ExtractString(obj, "inline_link_click_ctr")
        };
    }
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

    public static Datum FromJson(JToken? obj)
    {
        var id = FacebookLoaderBase.ExtractString(obj, "id");
        var name = FacebookLoaderBase.ExtractString(obj, "name");
        var createdTime = FacebookLoaderBase.ExtractString(obj, "created_time");
        var updatedTime = FacebookLoaderBase.ExtractString(obj, "updated_time");
        var previewShareableLink = FacebookLoaderBase.ExtractString(obj, "preview_shareable_link");

        // var previewsArray = FacebookLoaderBase.ExtractObjectArray(obj, "previews");
        var previewsArray = FacebookLoaderBase.ExtractObject(obj, "previews");
        var previews = Previews.FromJson(previewsArray);

        // var insightsArray = FacebookLoaderBase.ExtractObjectArray(obj, "insights");
        var insightsArray = FacebookLoaderBase.ExtractObject(obj, "insights");
        var insights = Insights.FromJson(insightsArray);

        return new Datum
        {
            Id = id,
            Name = name,
            CreatedTime = createdTime,
            UpdatedTime = updatedTime,
            PreviewShareableLink = previewShareableLink,
            Previews = previews,
            Insights = insights
        };
    }
}


class Insights
{
    public List<InsightDatum> Data { get; set; }
    public Paging Paging { get; set; }

    public static Insights FromJson(JToken? obj)
    {
        var data = FacebookLoaderBase.ExtractObjectArray(obj, "data");
        var list = new List<InsightDatum>();
        foreach (var item in data)
        {
            if (item is JObject dataObject)
                list.Add(InsightDatum.FromJson(dataObject));
        }

        var paging = Paging.FromJson(FacebookLoaderBase.ExtractObject(obj, "paging"));
        return new Insights
        {
            Data = list,
            Paging = paging
        };
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

class Root
{
    public List<Datum> Data { get; set; }
    public Paging Paging { get; set; }

    public static Root FromJson(JToken? obj)
    {
        var list = new List<Datum>();
        var data = FacebookLoaderBase.ExtractObjectArray(obj, "data");

        foreach (var item in data)
        {
            if (item is JObject o)
                list.Add(Datum.FromJson(o));
        }

        return new Root
        {
            Data = list,
            Paging = Paging.FromJson(FacebookLoaderBase.ExtractObject(obj, "paging"))
        };
    }
}
