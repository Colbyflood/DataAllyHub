namespace FacebookLoader.Content;

using System;
using System.Collections.Generic;

public class ActionData
{
    public int? Count { get; set; }
    public float? Value { get; set; }
    public float? CostPerAction { get; set; }
}

public class FacebookInsight
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
    public string Reach { get; set; }
    public string Frequency { get; set; }
    public float? Spend { get; set; }
    public int? Clicks { get; set; }
    public float? Cpc { get; set; }
    public float? Ctr { get; set; }
    public float? Cpm { get; set; }
    public int? Impressions { get; set; }
    public string ConversionRateRanking { get; set; }
    public string EngagementRateRanking { get; set; }
    public string QualityRanking { get; set; }
    public int? Video30SecWatched { get; set; }
    public float? VideoAvgTimeWatched { get; set; }
    public int? VideoP100Watched { get; set; }
    public int? VideoP25Watched { get; set; }
    public int? VideoP50Watched { get; set; }
    public int? VideoP75Watched { get; set; }
    public int? VideoP95Watched { get; set; }
    public int? VideoThruplayWatched { get; set; }
    public int? VideoPlay { get; set; }
    public float? CostPerThruplay { get; set; }
    public float? CostPerUniqueClick { get; set; }
    public int? OutboundClicks { get; set; }
    public float? OutboundClicksCtr { get; set; }
    public int? CostPerOutboundClick { get; set; }
    public float? InlineLinkClickCtr { get; set; }

    // ActionData fields
    public ActionData AddPayment { get; set; }
    public ActionData AddToCart { get; set; }
    public ActionData AddToWishlist { get; set; }
    public ActionData MobileAddPayment { get; set; }
    public ActionData MobileAddToCart { get; set; }
    public ActionData MobileAddToWishlist { get; set; }
    public ActionData MobileAppInstall { get; set; }
    public ActionData MobileCompleteRegistration { get; set; }
    public ActionData MobileInitiatedCheckout { get; set; }
    public ActionData MobilePurchase { get; set; }
    public ActionData AppInstall { get; set; }
    public ActionData AppUse { get; set; }
    public ActionData Comment { get; set; }
    public ActionData CompleteRegistration { get; set; }
    public ActionData ContactMobileApp { get; set; }
    public ActionData ContactOffline { get; set; }
    public ActionData ContactTotal { get; set; }
    public ActionData ContactWebsite { get; set; }
    public ActionData FindLocationMobile { get; set; }
    public ActionData FindLocationOffline { get; set; }
    public ActionData FindLocationTotal { get; set; }
    public ActionData FindLocationWebsite { get; set; }
    public ActionData InitiateCheckout { get; set; }
    public ActionData LandingPageView { get; set; }
    public ActionData Lead { get; set; }
    public ActionData Like { get; set; }
    public ActionData LinkClick { get; set; }
    public ActionData OffsiteConversionAddPayment { get; set; }
    public ActionData OffsiteConversionAddToCart { get; set; }
    public ActionData OffsiteConversionAddToWishlist { get; set; }
    public ActionData OffsiteConversionCompleteRegistration { get; set; }
    public ActionData OffsiteConversionInitiateCheckout { get; set; }
    public ActionData OffsiteConversionLead { get; set; }
    public ActionData OffsiteConversionPurchase { get; set; }
    public ActionData OffsiteConversionViewContent { get; set; }
    public ActionData OmniPurchase { get; set; }
    public ActionData OnsiteConversionPostSave { get; set; }
    public ActionData OnsiteConversionPurchase { get; set; }
    public ActionData PageEngagement { get; set; }
    public ActionData PhotoView { get; set; }
    public ActionData Post { get; set; }
    public ActionData PostEngagement { get; set; }
    public ActionData PostReaction { get; set; }
    public ActionData ScheduleMobileApp { get; set; }
    public ActionData ScheduleOffline { get; set; }
    public ActionData ScheduleTotal { get; set; }
    public ActionData ScheduleWebsite { get; set; }
    public ActionData StartTrialMobileApp { get; set; }
    public ActionData StartTrialOffline { get; set; }
    public ActionData StartTrialTotal { get; set; }
    public ActionData StartTrialWebsite { get; set; }
    public ActionData SubmitApplicationMobileApp { get; set; }
    public ActionData SubmitApplicationOffline { get; set; }
    public ActionData SubmitApplicationTotal { get; set; }
    public ActionData SubmitApplicationWebsite { get; set; }
    public ActionData SubscribeMobileApp { get; set; }
    public ActionData SubscribeTotal { get; set; }
    public ActionData SubscribeWebsite { get; set; }
    public ActionData VideoView { get; set; }
    public ActionData OnsitePurchases { get; set; }
}

public class FacebookAdInsight
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CreatedTime { get; set; }
    public string UpdatedTime { get; set; }
    public string PreviewShareableLink { get; set; }
    public List<string> Previews { get; set; }
    public List<FacebookInsight> Insights { get; set; }
}
