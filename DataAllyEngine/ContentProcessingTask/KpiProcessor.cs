namespace DataAllyEngine.ContentProcessingTask;

public class KpiProcessor
{
    private readonly Channel channel;
    private readonly DbConnection dbConnection;

    private readonly AppKpiProxy appKpiProxy;
    private readonly GeneralKpiProxy generalKpiProxy;
    private readonly EcommerceKpiProxy ecommerceKpiProxy;
    private readonly EcommerceChannelProxy ecommerceChannelProxy;
    private readonly EcommerceMobileProxy ecommerceMobileProxy;
    private readonly EcommerceTotalProxy ecommerceTotalProxy;
    private readonly EcommerceWebsiteProxy ecommerceWebsiteProxy;
    private readonly LeadGenKpiProxy leadgenKpiProxy;
    private readonly LeadGenApplicationProxy leadgenApplicationProxy;
    private readonly LeadGenAppointmentProxy leadgenAppointmentProxy;
    private readonly LeadGenContactProxy leadgenContactProxy;
    private readonly LeadGenLeadProxy leadgenLeadProxy;
    private readonly LeadGenLocationProxy leadgenLocationProxy;
    private readonly LeadGenRegistrationProxy leadgenRegistrationProxy;
    private readonly LeadGenSubscriptionProxy leadgenSubscriptionProxy;
    private readonly LeadGenTrialProxy leadgenTrialProxy;
    private readonly VideoKpiProxy videoKpiProxy;

    public KpiProcessor(Channel channel, DbConnection dbConnection)
    {
        channel = channel;
        dbConnection = dbConnection;

        appKpiProxy = new AppKpiProxy(dbConnection);
        generalKpiProxy = new GeneralKpiProxy(dbConnection);
        ecommerceKpiProxy = new EcommerceKpiProxy(dbConnection);
        ecommerceChannelProxy = new EcommerceChannelProxy(dbConnection);
        ecommerceMobileProxy = new EcommerceMobileProxy(dbConnection);
        ecommerceTotalProxy = new EcommerceTotalProxy(dbConnection);
        ecommerceWebsiteProxy = new EcommerceWebsiteProxy(dbConnection);
        leadgenKpiProxy = new LeadGenKpiProxy(dbConnection);
        leadgenApplicationProxy = new LeadGenApplicationProxy(dbConnection);
        leadgenAppointmentProxy = new LeadGenAppointmentProxy(dbConnection);
        leadgenContactProxy = new LeadGenContactProxy(dbConnection);
        leadgenLeadProxy = new LeadGenLeadProxy(dbConnection);
        leadgenLocationProxy = new LeadGenLocationProxy(dbConnection);
        leadgenRegistrationProxy = new LeadGenRegistrationProxy(dbConnection);
        leadgenSubscriptionProxy = new LeadGenSubscriptionProxy(dbConnection);
        leadgenTrialProxy = new LeadGenTrialProxy(dbConnection);
        videoKpiProxy = new VideoKpiProxy(dbConnection);
    }

    public void ImportKpis(Ad ad, FacebookInsight entry)
    {
        Console.WriteLine("Starting import kpis process");

        var effectiveDate = entry.DateStart;
        var createdDate = DateTime.Now;

        var appKpi = appKpiProxy.GetByAdAndEffectiveDate(ad.Id, effectiveDate);
        if (appKpi == null)
        {
            appKpi = new AppKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
            appKpiProxy.Save(appKpi);
        }

        var ecommerceKpi = ecommerceKpiProxy.GetByAdAndEffectiveDate(ad.Id, effectiveDate);
        if (ecommerceKpi == null)
        {
            ecommerceKpi = new EcommerceKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
            ecommerceKpiProxy.Save(ecommerceKpi);
        }

        var ecommerceChannel = ecommerceChannelProxy.GetByParent(ecommerceKpi.Id) ?? new EcommerceChannel
        {
            EcommerceKpiId = ecommerceKpi.Id
        };
        ecommerceChannelProxy.Save(ecommerceChannel);

        var ecommerceMobile = ecommerceMobileProxy.GetByParent(ecommerceKpi.Id) ?? new EcommerceMobile
        {
            EcommerceKpiId = ecommerceKpi.Id
        };
        ecommerceMobileProxy.Save(ecommerceMobile);

        var ecommerceTotal = ecommerceTotalProxy.GetByParent(ecommerceKpi.Id) ?? new EcommerceTotal
        {
            EcommerceKpiId = ecommerceKpi.Id
        };
        ecommerceTotalProxy.Save(ecommerceTotal);

        var ecommerceWebsite = ecommerceWebsiteProxy.GetByParent(ecommerceKpi.Id) ?? new EcommerceWebsite
        {
            EcommerceKpiId = ecommerceKpi.Id
        };
        ecommerceWebsiteProxy.Save(ecommerceWebsite);

        var generalKpi = generalKpiProxy.GetByAdAndEffectiveDate(ad.Id, effectiveDate);
        if (generalKpi == null)
        {
            generalKpi = new GeneralKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
        }

        generalKpi.IsActive = ad.AdDeactivated == null;
        generalKpiProxy.Save(generalKpi);

        var leadgenKpi = leadgenKpiProxy.GetByAdAndEffectiveDate(ad.Id, effectiveDate);
        if (leadgenKpi == null)
        {
            leadgenKpi = new LeadGenKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
            leadgenKpiProxy.Save(leadgenKpi);
        }

        var leadgenApplication = leadgenApplicationProxy.GetByParent(leadgenKpi.Id) ?? new LeadGenApplication
        {
            LeadGenKpiId = leadgenKpi.Id
        };
        leadgenApplicationProxy.Save(leadgenApplication);

        var leadgenAppointment = leadgenAppointmentProxy.GetByParent(leadgenKpi.Id) ?? new LeadGenAppointment
        {
            LeadGenKpiId = leadgenKpi.Id
        };
        leadgenAppointmentProxy.Save(leadgenAppointment);

        var leadgenContact = leadgenContactProxy.GetByParent(leadgenKpi.Id) ?? new LeadGenContact
        {
            LeadGenKpiId = leadgenKpi.Id
        };
        leadgenContactProxy.Save(leadgenContact);

        var leadgenLead = leadgenLeadProxy.GetByParent(leadgenKpi.Id) ?? new LeadGenLead
        {
            LeadGenKpiId = leadgenKpi.Id
        };
        leadgenLeadProxy.Save(leadgenLead);

        var leadgenLocation = leadgenLocationProxy.GetByParent(leadgenKpi.Id) ?? new LeadGenLocation
        {
            LeadGenKpiId = leadgenKpi.Id
        };
        leadgenLocationProxy.Save(leadgenLocation);

        var leadgenRegistration = leadgenRegistrationProxy.GetByParent(leadgenKpi.Id) ?? new LeadGenRegistration
        {
            LeadGenKpiId = leadgenKpi.Id
        };
        leadgenRegistrationProxy.Save(leadgenRegistration);

        var leadgenSubscription = leadgenSubscriptionProxy.GetByParent(leadgenKpi.Id) ?? new LeadGenSubscription
        {
            LeadGenKpiId = leadgenKpi.Id
        };
        leadgenSubscriptionProxy.Save(leadgenSubscription);

        var leadgenTrial = leadgenTrialProxy.GetByParent(leadgenKpi.Id) ?? new LeadGenTrial
        {
            LeadGenKpiId = leadgenKpi.Id
        };
        leadgenTrialProxy.Save(leadgenTrial);

        var videoKpi = videoKpiProxy.GetByAdAndEffectiveDate(ad.Id, effectiveDate);
        if (videoKpi == null)
        {
            videoKpi = new VideoKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
            videoKpiProxy.Save(videoKpi);
        }

        LoadAppKpi(entry, appKpi);
        LoadEcommerceKpi(entry, ecommerceChannel, ecommerceMobile, ecommerceTotal, ecommerceWebsite);
        LoadGeneralKpi(entry, generalKpi);
        LoadLeadgenKpi(entry, leadgenApplication, leadgenAppointment, leadgenContact, leadgenLead,
            leadgenLocation, leadgenRegistration, leadgenSubscription, leadgenTrial);
        LoadVideoKpi(entry, videoKpi);

        // Final save with updated timestamp
        appKpi.Updated = createdDate;
        appKpiProxy.Save(appKpi);

        ecommerceKpi.Updated = createdDate;
        ecommerceKpiProxy.Save(ecommerceKpi);
        ecommerceChannelProxy.Save(ecommerceChannel);
        ecommerceMobileProxy.Save(ecommerceMobile);
        ecommerceTotalProxy.Save(ecommerceTotal);
        ecommerceWebsiteProxy.Save(ecommerceWebsite);

        generalKpi.Updated = createdDate;
        generalKpiProxy.Save(generalKpi);

        leadgenKpi.Updated = createdDate;
        leadgenKpiProxy.Save(leadgenKpi);
        leadgenApplicationProxy.Save(leadgenApplication);
        leadgenAppointmentProxy.Save(leadgenAppointment);
        leadgenContactProxy.Save(leadgenContact);
        leadgenLeadProxy.Save(leadgenLead);
        leadgenLocationProxy.Save(leadgenLocation);
        leadgenRegistrationProxy.Save(leadgenRegistration);
        leadgenSubscriptionProxy.Save(leadgenSubscription);
        leadgenTrialProxy.Save(leadgenTrial);

        videoKpi.Updated = createdDate;
        videoKpiProxy.Save(videoKpi);
    }

    private static void LoadGeneralKpi(FacebookInsight entry, GeneralKpi generalKpi)
    {
        generalKpi.AdRecallLift = null;
        generalKpi.AdRecallRate = null;
        generalKpi.AllClicks = entry.Clicks;
        generalKpi.AllCpc = entry.Cpc;
        generalKpi.AllCtr = entry.Ctr;
        generalKpi.ConversionRateRanking = entry.ConversionRateRanking;
        generalKpi.Cpm = entry.Cpm;
        generalKpi.EngagementRanking = entry.EngagementRateRanking;
        generalKpi.Frequency = entry.Frequency;
        generalKpi.Impressions = entry.Impressions;
        generalKpi.LinkClickTotal = entry.LinkClick.Count;
        generalKpi.OutboundClicks = entry.OutboundClicks;
        generalKpi.OutboundCtr = entry.OutboundClicksCtr;
        generalKpi.OutboundLinkClickCpc = entry.OutboundClicks;
        generalKpi.PageEngagements = entry.PageEngagement.Count;
        generalKpi.PageLikes = entry.Like.Count;
        generalKpi.PhotosViewed = entry.PhotoView.Count;
        generalKpi.PostComments = entry.Comment.Count;
        generalKpi.PostReactions = entry.PostReaction.Count;
        generalKpi.PostSaves = entry.OnsiteConversionPostSave.Count;
        generalKpi.PostShares = entry.Post.Count;
        generalKpi.QualityRanking = entry.QualityRanking;
        generalKpi.Reach = entry.Reach;
        generalKpi.Spend = entry.Spend;
        generalKpi.InlineCtr = entry.InlineLinkClickCtr;
        generalKpi.CostPerLinkClick = entry.LinkClick.CostPerAction;
        generalKpi.LandingPageView = entry.LandingPageView.Count;
        generalKpi.WebsiteViewContent = entry.OffsiteConversionViewContent.Count;
    }

    private static void LoadAppKpi(FacebookInsight entry, AppKpi appKpi)
    {
        appKpi.AppOpen = entry.AppUse.Count;
        appKpi.ConversionRate = null;

        if (entry.LinkClick?.Count > 0 && entry.AppInstall?.Count > 0)
        {
            try
            {
                appKpi.InstallsRate = (entry.LinkClick.Count / (double) entry.AppInstall.Count) * 100.0;
            }
            catch (DivideByZeroException)
            {
                appKpi.InstallsRate = null;
            }
        }

        appKpi.CostPerAppInstall = entry.AppInstall.CostPerAction;
    }

    private static void LoadVideoKpi(FacebookInsight entry, VideoKpi videoKpi)
    {
        videoKpi.AverageWatchSeconds = entry.VideoAvgTimeWatched;
        videoKpi.Play100Percent = entry.VideoP100Watched;
        videoKpi.Play25Percent = entry.VideoP25Watched;
        videoKpi.Play3Seconds = entry.VideoView.Count;
        videoKpi.Play50Percent = entry.VideoP50Watched;
        videoKpi.Play75Percent = entry.VideoP75Watched;
        videoKpi.Play95Percent = entry.VideoP95Watched;
        videoKpi.PlayThru = entry.VideoThruplayWatched;
        videoKpi.PlayTotal = entry.VideoPlay;
        videoKpi.Watched30Seconds = entry.Video30SecWatched;
    }

    private static void LoadEcommerceKpi(FacebookInsight entry, EcommerceChannel ecommerceChannel,
        EcommerceMobile ecommerceMobile, EcommerceTotal ecommerceTotal,
        EcommerceWebsite ecommerceWebsite)
    {
        ecommerceChannel.ChannelAddToCart = entry.AddToCart.Count;
        ecommerceChannel.ChannelAddToCartValue = entry.AddToCart.Value;
        ecommerceChannel.ChannelAddToWishlist = entry.AddToWishlist.Count;
        ecommerceChannel.ChannelAddToWishlistValue = entry.AddToWishlist.Value;
        ecommerceChannel.ChannelCheckoutInitiated = entry.InitiateCheckout.Count;
        ecommerceChannel.ChannelCheckoutInitiatedValue = entry.InitiateCheckout.Value;
        ecommerceChannel.ChannelCostPerAddToCart = entry.AddToCart.CostPerAction;
        ecommerceChannel.ChannelPurchases = entry.OnsitePurchases.Count;
        ecommerceChannel.ChannelPurchasesValue = entry.OnsitePurchases.Value;

        if (entry.OnsitePurchases?.Value > 0 && entry.Spend > 0)
        {
            try
            {
                ecommerceChannel.ChannelRoa = entry.OnsitePurchases.Value / entry.Spend;
            }
            catch (DivideByZeroException)
            {
                ecommerceChannel.ChannelRoa = null;
            }
        }

        if (entry.OnsitePurchases?.Count > 0 && entry.Spend > 0)
        {
            try
            {
                ecommerceChannel.CostPerChannelPurchases = entry.Spend / entry.OnsitePurchases.Count;
            }
            catch (DivideByZeroException)
            {
                ecommerceChannel.CostPerChannelPurchases = null;
            }
        }

        ecommerceMobile.MobileAppAddPaymentInfo = entry.MobileAddPayment.Count;
        ecommerceMobile.MobileAppAddPaymentInfoValue = entry.MobileAddPayment.CostPerAction;
        ecommerceMobile.MobileAppAddToCart = entry.MobileAddToCart.Count;
        ecommerceMobile.MobileAppAddToCartValue = entry.MobileAddToCart.CostPerAction;
        ecommerceMobile.MobileAppAddToWishlist = entry.MobileAddToWishlist.Count;
        ecommerceMobile.MobileAppAddToWishlistValue = entry.MobileAddToWishlist.CostPerAction;
        ecommerceMobile.MobileAppCheckoutInitiated = entry.MobileInitiatedCheckout.Count;
        ecommerceMobile.MobileAppCheckoutInitiatedValue = entry.MobileInitiatedCheckout.CostPerAction;

        // Unknown mappings – placeholders
        ecommerceTotal.CostPerTotalAddPaymentInfo = null;
        ecommerceTotal.CostPerTotalAddToCart = null;
        ecommerceTotal.CostPerTotalAddToWishlist = null;
        ecommerceTotal.CostPerTotalCheckoutInitiated = null;
        ecommerceTotal.TotalAddPaymentInfo = null;
        ecommerceTotal.TotalAddPaymentInfoValue = null;
        ecommerceTotal.TotalAddToCart = null;
        ecommerceTotal.TotalAddToCartValue = null;
        ecommerceTotal.TotalAddToWishlist = null;
        ecommerceTotal.TotalAddToWishlistValue = null;
        ecommerceTotal.TotalCheckoutInitiated = null;
        ecommerceTotal.TotalCheckoutInitiatedValue = null;
        ecommerceTotal.TotalPurchases = null;
        ecommerceTotal.TotalPurchasesValue = null;
        ecommerceTotal.TotalRoa = null;

        ecommerceWebsite.CostPerWebsiteAddPaymentInfo = entry.OffsiteConversionAddPayment.CostPerAction;
        ecommerceWebsite.CostPerWebsiteAddToCart = entry.OffsiteConversionAddToCart.CostPerAction;
        ecommerceWebsite.CostPerWebsiteAddToWishlist = entry.OffsiteConversionAddToWishlist.CostPerAction;
        ecommerceWebsite.CostPerWebsiteCheckoutInitiated = entry.OffsiteConversionInitiateCheckout.CostPerAction;
        ecommerceWebsite.CostPerWebsitePurchases = entry.OffsiteConversionPurchase.CostPerAction;
        ecommerceWebsite.WebsiteAddPaymentInfo = entry.OffsiteConversionAddPayment.Count;
        ecommerceWebsite.WebsiteAddPaymentInfoValue = entry.OffsiteConversionAddPayment.Value;
        ecommerceWebsite.WebsiteAddToCart = entry.OffsiteConversionAddToCart.Count;
        ecommerceWebsite.WebsiteAddToCartValue = entry.OffsiteConversionAddToCart.Value;
        ecommerceWebsite.WebsiteAddToWishlist = entry.OffsiteConversionAddToWishlist.Count;
        ecommerceWebsite.WebsiteAddToWishlistValue = entry.OffsiteConversionAddToWishlist.Value;
        ecommerceWebsite.WebsiteCheckoutInitiated = entry.OffsiteConversionInitiateCheckout.Count;
        ecommerceWebsite.WebsiteCheckoutInitiatedValue = entry.OffsiteConversionInitiateCheckout.Value;
        ecommerceWebsite.WebsitePurchases = entry.OffsiteConversionPurchase.Count;
        ecommerceWebsite.WebsitePurchasesValue = entry.OffsiteConversionPurchase.Value;
        ecommerceWebsite.WebsiteRoa = null;
    }

    private static void LoadLeadgenKpi(FacebookInsight entry,
        LeadGenApplication leadgenApplication,
        LeadGenAppointment leadgenAppointment,
        LeadGenContact leadgenContact,
        LeadGenLead leadgenLead,
        LeadGenLocation leadgenLocation,
        LeadGenRegistration leadgenRegistration,
        LeadGenSubscription leadgenSubscription,
        LeadGenTrial leadgenTrial)
    {
        leadgenApplication.CostPerMobileAppSubmitApplications = entry.SubmitApplicationMobileApp.CostPerAction;
        leadgenApplication.CostPerOfflineSubmitApplications = entry.SubmitApplicationOffline.CostPerAction;
        leadgenApplication.CostPerSubmitApplications = entry.SubmitApplicationTotal.CostPerAction;
        leadgenApplication.CostPerWebsiteSubmitApplications = entry.SubmitApplicationWebsite.CostPerAction;
        leadgenApplication.MobileAppSubmitApplications = entry.SubmitApplicationMobileApp.Count;
        leadgenApplication.MobileAppSubmitApplicationsValue = entry.SubmitApplicationMobileApp.Value;
        leadgenApplication.OfflineSubmitApplications = entry.SubmitApplicationOffline.Count;
        leadgenApplication.OfflineSubmitApplicationsValue = entry.SubmitApplicationOffline.Value;
        leadgenApplication.SubmitApplications = entry.SubmitApplicationTotal.Count;
        leadgenApplication.SubmitApplicationsValue = entry.SubmitApplicationTotal.Value;
        leadgenApplication.WebsiteSubmitApplications = entry.SubmitApplicationWebsite.Count;
        leadgenApplication.WebsiteSubmitApplicationsValue = entry.SubmitApplicationWebsite.Value;

        leadgenAppointment.AppointmentsScheduled = entry.ScheduleTotal.Count;
        leadgenAppointment.AppointmentsScheduledValue = entry.ScheduleTotal.Value;
        leadgenAppointment.CostPerAppointmentScheduled = entry.ScheduleTotal.CostPerAction;
        leadgenAppointment.CostPerMobileAppAppointmentsScheduled = entry.ScheduleMobileApp.CostPerAction;
        leadgenAppointment.CostPerOfflineAppointmentsScheduled = entry.ScheduleOffline.CostPerAction;
        leadgenAppointment.CostPerWebsiteAppointmentsScheduled = entry.ScheduleWebsite.CostPerAction;
        leadgenAppointment.MobileAppAppointmentsScheduled = entry.ScheduleMobileApp.Count;
        leadgenAppointment.MobileAppAppointmentsScheduledValue = entry.ScheduleMobileApp.Value;
        leadgenAppointment.OfflineAppointmentsScheduled = entry.ScheduleOffline.Count;
        leadgenAppointment.OfflineAppointmentsScheduledValue = entry.ScheduleOffline.Value;
        leadgenAppointment.WebsiteAppointmentsScheduled = entry.ScheduleWebsite.Count;
        leadgenAppointment.WebsiteAppointmentsScheduledValue = entry.ScheduleWebsite.Value;

        leadgenContact.Contacts = entry.ContactTotal.Count;
        leadgenContact.ContactsValue = entry.ContactTotal.Value;
        leadgenContact.CostPerContacts = entry.ContactTotal.CostPerAction;
        leadgenContact.CostPerMobileAppContacts = entry.ContactMobileApp.CostPerAction;
        leadgenContact.CostPerOfflineContacts = entry.ContactOffline.CostPerAction;
        leadgenContact.CostPerWebsiteContacts = entry.ContactWebsite.CostPerAction;
        leadgenContact.MobileAppContacts = entry.ContactMobileApp.Count;
        leadgenContact.MobileAppContactsValue = entry.ContactMobileApp.Value;
        leadgenContact.OfflineContacts = entry.ContactOffline.Count;
        leadgenContact.OfflineContactsValue = entry.ContactOffline.Value;
        leadgenContact.WebsiteContacts = entry.ContactWebsite.Count;
        leadgenContact.WebsiteContactsValue = entry.ContactWebsite.Value;

        leadgenLead.ChannelLeadGenFormsSubmitted = null;
        leadgenLead.CostPerLead = entry.Lead.CostPerAction;
        leadgenLead.CostPerWebsiteLead = null;
        leadgenLead.Leads = entry.Lead.Count;
        leadgenLead.WebsiteLeads = null;
        leadgenLead.WebsiteLeadsValue = null;

        leadgenLocation.CostPerFindLocations = entry.FindLocationTotal.CostPerAction;
        leadgenLocation.CostPerMobileAppFindLocations = entry.FindLocationMobile.CostPerAction;
        leadgenLocation.CostPerOfflineFindLocations = entry.FindLocationOffline.CostPerAction;
        leadgenLocation.CostPerWebsiteFindLocations = entry.FindLocationWebsite.CostPerAction;
        leadgenLocation.FindLocations = entry.FindLocationTotal.Count;
        leadgenLocation.FindLocationsValue = entry.FindLocationTotal.Value;
        leadgenLocation.MobileAppFindLocations = entry.FindLocationMobile.Count;
        leadgenLocation.MobileAppFindLocationsValue = entry.FindLocationMobile.Value;
        leadgenLocation.OfflineFindLocations = entry.FindLocationOffline.Count;
        leadgenLocation.OfflineFindLocationsValue = entry.FindLocationOffline.Value;
        leadgenLocation.WebsiteFindLocations = entry.FindLocationWebsite.Count;
        leadgenLocation.WebsiteFindLocationsValue = entry.FindLocationWebsite.Value;

        leadgenRegistration.CostPerRegistrationsCompleted = entry.CompleteRegistration.CostPerAction;
        leadgenRegistration.CostPerWebsiteRegistrationsCompleted = entry.SubmitApplicationWebsite.CostPerAction;
        leadgenRegistration.MobileAppRegistrationsCompleted = entry.MobileCompleteRegistration.CostPerAction;
        leadgenRegistration.MobileAppRegistrationsCompletedValue = entry.MobileCompleteRegistration.Value;
        leadgenRegistration.RegistrationsCompleted = entry.CompleteRegistration.Count;
        leadgenRegistration.RegistrationsCompletedValue = entry.CompleteRegistration.Value;
        leadgenRegistration.WebsiteRegistrationsCompleted = entry.SubmitApplicationWebsite.Count;
        leadgenRegistration.WebsiteRegistrationsCompletedValue = entry.SubmitApplicationWebsite.Value;

        leadgenSubscription.CostPerMobileAppSubscriptions = entry.SubscribeMobileApp.CostPerAction;
        leadgenSubscription.CostPerSubscriptions = entry.SubscribeTotal.CostPerAction;
        leadgenSubscription.CostPerWebsiteSubscriptions = entry.SubscribeWebsite.CostPerAction;
        leadgenSubscription.MobileAppSubscriptions = entry.SubscribeMobileApp.Count;
        leadgenSubscription.MobileAppSubscriptionsValue = entry.SubscribeMobileApp.Value;
        leadgenSubscription.Subscriptions = entry.SubscribeTotal.Count;
        leadgenSubscription.SubscriptionsValue = entry.SubscribeTotal.Value;
        leadgenSubscription.WebsiteSubscriptions = entry.SubscribeWebsite.Count;
        leadgenSubscription.WebsiteSubscriptionsValue = entry.SubscribeWebsite.Value;

        leadgenTrial.MobileTrialsStarted = entry.StartTrialMobileApp.Count;
        leadgenTrial.MobileTrialsStartedValue = entry.StartTrialMobileApp.Value;
        leadgenTrial.TotalTrialsStarted = entry.StartTrialTotal.Value;
        leadgenTrial.TotalTrialsStartedValue = entry.StartTrialTotal.Value;
        leadgenTrial.WebsiteTrialsStarted = entry.StartTrialWebsite.Count;
        leadgenTrial.WebsiteTrialsStartedValue = entry.StartTrialWebsite.Value;
    }

    public static bool IsPositiveInteger(int? value)
    {
        return value.HasValue && value.Value > 0;
    }

    public static bool IsPositiveDecimal(double? value)
    {
        return value.HasValue && value.Value > 0.0;
    }
}