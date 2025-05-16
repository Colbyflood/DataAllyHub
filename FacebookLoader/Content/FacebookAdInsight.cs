namespace FacebookLoader.Content;

using System;
using System.Collections.Generic;

public class ActionData
{
    public int? Count { get; }
    public float? Value { get; }
    public float? CostPerAction { get; }
    
    public ActionData(int? count, float? value, float? costPerAction)
    {
        Count = count;
        Value = value;
        CostPerAction = costPerAction;
    }
}

public class FacebookInsight
{
    public string DateStart { get; }
    public string DateStop { get; }
    public string AccountId { get; }
    public string AccountName { get; }
    public string AccountCurrency { get; }
    public string AttributionSetting { get; }
    public string OptimizationGoal { get; }
    public string CampaignId { get; }
    public string CampaignName { get; }
    public string Objective { get; }
    public string BuyingType { get; }
    public string AdsetId { get; }
    public string AdsetName { get; }
    public string AdId { get; }
    public string AdName { get; }
    public string Reach { get; }
    public string Frequency { get; }
    public float? Spend { get; }
    public int? Clicks { get; }
    public float? Cpc { get; }
    public float? Ctr { get; }
    public float? Cpm { get; }
    public int? Impressions { get; }
    public string ConversionRateRanking { get; }
    public string EngagementRateRanking { get; }
    public string QualityRanking { get; }
    public int? Video30SecWatched { get; }
    public float? VideoAvgTimeWatched { get; }
    public int? VideoP100Watched { get; }
    public int? VideoP25Watched { get; }
    public int? VideoP50Watched { get; }
    public int? VideoP75Watched { get; }
    public int? VideoP95Watched { get; }
    public int? VideoThruplayWatched { get; }
    public int? VideoPlay { get; }
    public float? CostPerThruplay { get; }
    public float? CostPerUniqueClick { get; }
    public int? OutboundClicks { get; }
    public float? OutboundClicksCtr { get; }
    public float? CostPerOutboundClick { get; }
    public float? InlineLinkClickCtr { get; }

    // ActionData fields
    public ActionData AddPayment { get; }
    public ActionData AddToCart { get; }
    public ActionData AddToWishlist { get; }
    public ActionData MobileAddPayment { get; }
    public ActionData MobileAddToCart { get; }
    public ActionData MobileAddToWishlist { get; }
    public ActionData MobileAppInstall { get; }
    public ActionData MobileCompleteRegistration { get; }
    public ActionData MobileInitiatedCheckout { get; }
    public ActionData MobilePurchase { get; }
    public ActionData AppInstall { get; }
    public ActionData AppUse { get; }
    public ActionData Comment { get; }
    public ActionData CompleteRegistration { get; }
    public ActionData ContactMobileApp { get; }
    public ActionData ContactOffline { get; }
    public ActionData ContactTotal { get; }
    public ActionData ContactWebsite { get; }
    public ActionData FindLocationMobile { get; }
    public ActionData FindLocationOffline { get; }
    public ActionData FindLocationTotal { get; }
    public ActionData FindLocationWebsite { get; }
    public ActionData InitiateCheckout { get; }
    public ActionData LandingPageView { get; }
    public ActionData Lead { get; }
    public ActionData Like { get; }
    public ActionData LinkClick { get; }
    public ActionData OffsiteConversionAddPayment { get; }
    public ActionData OffsiteConversionAddToCart { get; }
    public ActionData OffsiteConversionAddToWishlist { get; }
    public ActionData OffsiteConversionCompleteRegistration { get; }
    public ActionData OffsiteConversionInitiateCheckout { get; }
    public ActionData OffsiteConversionLead { get; }
    public ActionData OffsiteConversionPurchase { get; }
    public ActionData OffsiteConversionViewContent { get; }
    public ActionData OmniPurchase { get; }
    public ActionData OnsiteConversionPostSave { get; }
    public ActionData OnsiteConversionPurchase { get; }
    public ActionData PageEngagement { get; }
    public ActionData PhotoView { get; }
    public ActionData Post { get; }
    public ActionData PostEngagement { get; }
    public ActionData PostReaction { get; }
    public ActionData ScheduleMobileApp { get; }
    public ActionData ScheduleOffline { get; }
    public ActionData ScheduleTotal { get; }
    public ActionData ScheduleWebsite { get; }
    public ActionData StartTrialMobileApp { get; }
    public ActionData StartTrialOffline { get; }
    public ActionData StartTrialTotal { get; }
    public ActionData StartTrialWebsite { get; }
    public ActionData SubmitApplicationMobileApp { get; }
    public ActionData SubmitApplicationOffline { get; }
    public ActionData SubmitApplicationTotal { get; }
    public ActionData SubmitApplicationWebsite { get; }
    public ActionData SubscribeMobileApp { get; }
    public ActionData SubscribeTotal { get; }
    public ActionData SubscribeWebsite { get; }
    public ActionData VideoView { get; }
    public ActionData OnsitePurchases { get; }

    // Oleksii - look here
    public ActionData TotalAddPaymentInfo { get; }
    public ActionData TotalAddToCart { get; }
    public ActionData TotalAddToWishlist { get; }
    public ActionData TotalCheckoutInitiated { get; }
    public ActionData TotalPurchases { get; }
    
    public FacebookInsight(string DateStart, string DateStop, string AccountId, string AccountName,
        string AccountCurrency, string AttributionSetting, string OptimizationGoal, string CampaignId,
        string CampaignName, string Objective, string BuyingType, string AdsetId, string AdsetName,
        string AdId, string AdName, string Reach, string Frequency, float? Spend, int? Clicks, float? Cpc,
        float? Ctr, float? Cpm, int? Impressions, string ConversionRateRanking, string EngagementRateRanking,
        string QualityRanking, int? Video30SecWatched, float? VideoAvgTimeWatched, int? VideoP100Watched,
        int? VideoP25Watched, int? VideoP50Watched, int? VideoP75Watched, int? VideoP95Watched,
        int? VideoThruplayWatched, int? VideoPlay, float? CostPerThruplay, float? CostPerUniqueClick,
        int? OutboundClicks, float? OutboundClicksCtr, float? CostPerOutboundClick, float? InlineLinkClickCtr,
        ActionData AddPayment, ActionData AddToCart, ActionData AddToWishlist, ActionData MobileAddPayment,
        ActionData MobileAddToCart, ActionData MobileAddToWishlist, ActionData MobileAppInstall,
        ActionData MobileCompleteRegistration, ActionData MobileInitiatedCheckout, ActionData MobilePurchase,
        ActionData AppInstall, ActionData AppUse, ActionData Comment, ActionData CompleteRegistration,
        ActionData ContactMobileApp, ActionData ContactOffline, ActionData ContactTotal,
        ActionData ContactWebsite, ActionData FindLocationMobile, ActionData FindLocationOffline,
        ActionData FindLocationTotal, ActionData FindLocationWebsite, ActionData InitiateCheckout,
        ActionData LandingPageView, ActionData Lead, ActionData Like, ActionData LinkClick,
        ActionData OffsiteConversionAddPayment, ActionData OffsiteConversionAddToCart,
        ActionData OffsiteConversionAddToWishlist, ActionData OffsiteConversionCompleteRegistration,
        ActionData OffsiteConversionInitiateCheckout, ActionData OffsiteConversionLead,
        ActionData OffsiteConversionPurchase, ActionData OffsiteConversionViewContent,
        ActionData OmniPurchase, ActionData OnsiteConversionPostSave,
        ActionData OnsiteConversionPurchase, ActionData PageEngagement, ActionData PhotoView,
        ActionData Post, ActionData PostEngagement, ActionData PostReaction, ActionData ScheduleMobileApp,
        ActionData ScheduleOffline, ActionData ScheduleTotal, ActionData ScheduleWebsite,
        ActionData StartTrialMobileApp, ActionData StartTrialOffline, ActionData StartTrialTotal,
        ActionData StartTrialWebsite, ActionData SubmitApplicationMobileApp,
        ActionData SubmitApplicationOffline, ActionData SubmitApplicationTotal,
        ActionData SubmitApplicationWebsite, ActionData SubscribeMobileApp, ActionData SubscribeTotal,
        ActionData SubscribeWebsite, ActionData VideoView, ActionData OnsitePurchases, 
        // Oleksii - look here
        ActionData TotalAddPaymentInfo, ActionData TotalAddToCart, 
        ActionData TotalAddToWishlist, ActionData TotalCheckoutInitiated, ActionData TotalPurchases 
        )
    {
        this.DateStart = DateStart;
        this.DateStop = DateStop;
        this.AccountId = AccountId;
        this.AccountName = AccountName;
        this.AccountCurrency = AccountCurrency;
        this.AttributionSetting = AttributionSetting;
        this.OptimizationGoal = OptimizationGoal;
        this.CampaignId = CampaignId;
        this.CampaignName = CampaignName;
        this.Objective = Objective;
        this.BuyingType = BuyingType;
        this.AdsetId = AdsetId;
        this.AdsetName = AdsetName;
        this.AdId = AdId;
        this.AdName = AdName;
        this.Reach = Reach;
        this.Frequency = Frequency;
        this.Spend = Spend;
        this.Clicks = Clicks;
        this.Cpc = Cpc;
        this.Ctr = Ctr;
        this.Cpm = Cpm;
        this.Impressions = Impressions;
        this.ConversionRateRanking = ConversionRateRanking;
        this.EngagementRateRanking = EngagementRateRanking;
        this.Video30SecWatched = Video30SecWatched;
        this.VideoAvgTimeWatched = VideoAvgTimeWatched;
        this.VideoP100Watched = VideoP100Watched;
        this.VideoP25Watched = VideoP25Watched;
        this.VideoP50Watched = VideoP50Watched;
        this.VideoP75Watched = VideoP75Watched;
        this.VideoP95Watched = VideoP95Watched;
        this.VideoThruplayWatched = VideoThruplayWatched;
        this.VideoPlay = VideoPlay;
        this.CostPerThruplay = CostPerThruplay;
        this.CostPerUniqueClick = CostPerUniqueClick;
        this.OutboundClicks = OutboundClicks;
        this.OutboundClicksCtr = OutboundClicksCtr;
        this.CostPerOutboundClick = CostPerOutboundClick;
        this.InlineLinkClickCtr = InlineLinkClickCtr;
        this.AddPayment = AddPayment;
        this.AddToCart = AddToCart;
        this.AddToWishlist = AddToWishlist;
        this.MobileAddPayment = MobileAddPayment;
        this.MobileAddToCart = MobileAddToCart;
        this.MobileAddToWishlist = MobileAddToWishlist;
        this.MobileAppInstall = MobileAppInstall;
        this.MobileCompleteRegistration = MobileCompleteRegistration;
        this.MobileInitiatedCheckout = MobileInitiatedCheckout;
        this.MobilePurchase = MobilePurchase;
        this.AppInstall = AppInstall;
        this.AppUse = AppUse;
        this.Comment = Comment;
        this.CompleteRegistration = CompleteRegistration;
        this.ContactMobileApp = ContactMobileApp;
        this.ContactOffline = ContactOffline;
        this.ContactTotal = ContactTotal;
        this.ContactWebsite = ContactWebsite;
        this.FindLocationMobile = FindLocationMobile;
        this.FindLocationOffline = FindLocationOffline;
        this.FindLocationTotal = FindLocationTotal;
        this.FindLocationWebsite = FindLocationWebsite;
        this.InitiateCheckout = InitiateCheckout;
        this.LandingPageView = LandingPageView;
        this.Lead = Lead;
        this.Like = Like;
        this.LinkClick = LinkClick;
        this.OffsiteConversionAddPayment = OffsiteConversionAddPayment;
        this.OffsiteConversionAddToCart = OffsiteConversionAddToCart;
        this.OffsiteConversionAddToWishlist = OffsiteConversionAddToWishlist;
        this.OffsiteConversionCompleteRegistration = OffsiteConversionCompleteRegistration;
        this.OffsiteConversionInitiateCheckout = OffsiteConversionInitiateCheckout;
        this.OffsiteConversionLead = OffsiteConversionLead;
        this.OffsiteConversionPurchase = OffsiteConversionPurchase;
        this.OffsiteConversionViewContent = OffsiteConversionViewContent;
        this.OmniPurchase = OmniPurchase;
        this.OnsiteConversionPostSave = OnsiteConversionPostSave;
        this.OnsiteConversionPurchase = OnsiteConversionPurchase;
        this.PageEngagement = PageEngagement;
        this.PhotoView = PhotoView;
        this.Post = Post;
        this.PostEngagement = PostEngagement;
        this.PostReaction = PostReaction;
        this.ScheduleMobileApp = ScheduleMobileApp;
        this.ScheduleOffline = ScheduleOffline;
        this.ScheduleTotal = ScheduleTotal;
        this.ScheduleWebsite = ScheduleWebsite;
        this.StartTrialMobileApp = StartTrialMobileApp;
        this.StartTrialOffline = StartTrialOffline;
        this.StartTrialTotal = StartTrialTotal;
        this.StartTrialWebsite = StartTrialWebsite;
        this.SubmitApplicationMobileApp = SubmitApplicationMobileApp;
        this.SubmitApplicationOffline = SubmitApplicationOffline;
        this.SubmitApplicationTotal = SubmitApplicationTotal;
        this.SubmitApplicationWebsite = SubmitApplicationWebsite;
        this.SubscribeMobileApp = SubscribeMobileApp;
        this.SubscribeTotal = SubscribeTotal;
        this.SubscribeWebsite = SubscribeWebsite;
        this.VideoView = VideoView;
        this.OnsitePurchases = OnsitePurchases;
        
        // Oleksii - look here
        this.TotalAddPaymentInfo = TotalAddPaymentInfo;
        this.TotalAddToCart = TotalAddToCart;
        this.TotalAddToWishlist = TotalAddToWishlist;
        this.TotalCheckoutInitiated = TotalCheckoutInitiated;
        this.TotalPurchases = TotalPurchases;
    }
}

public class FacebookAdInsight
{
    public string Id { get; }
    public string Name { get; }
    public string CreatedTime { get; }
    public string UpdatedTime { get; }
    public string PreviewShareableLink { get; }
    public List<string> Previews { get; }
    public List<FacebookInsight> Insights { get; }
    
    public FacebookAdInsight(string id, string name, string createdTime, string updatedTime, string previewShareableLink, List<string> previews, List<FacebookInsight> insights)
    {
        Id = id;
        Name = name;
        CreatedTime = createdTime;
        UpdatedTime = updatedTime;
        PreviewShareableLink = previewShareableLink;
        Previews = previews;
        Insights = insights;
    }
}
