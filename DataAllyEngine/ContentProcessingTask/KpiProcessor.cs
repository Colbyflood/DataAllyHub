using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Content;

namespace DataAllyEngine.ContentProcessingTask;

public class KpiProcessor
{
    private readonly Channel channel;

    private readonly IKpiProxy kpiProxy;
    private readonly ILogger logger;
    
    public KpiProcessor(Channel channel, IKpiProxy kpiProxy, ILogger logger)
    {
        this.channel = channel;
        this.kpiProxy = kpiProxy;
        this.logger = logger;
    }

    private static DateTime ParseEffectiveDate(string effectiveDateString)
    {
        if (DateTime.TryParse(effectiveDateString, out var dateTime))
        {
            return dateTime.Date;
        }
        else
        {
            throw new FormatException("Invalid effective date");
        }
    }
    
    public void ImportKpis(Ad ad, FacebookInsight entry)
    {
        Console.WriteLine("Starting import kpis process");

        var effectiveDate = ParseEffectiveDate(entry.DateStart);
        var createdDate = DateTime.Now;

        var appKpi = kpiProxy.GetAppKpiByAdIdAndEffectiveDate(ad.Id, effectiveDate);
        if (appKpi == null)
        {
            appKpi = new AppKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
            kpiProxy.WriteAppKpi(appKpi);
        }

        var ecommerceKpi = kpiProxy.GetEcommerceKpiByAdIdAndEffectiveDate(ad.Id, effectiveDate);
        if (ecommerceKpi == null)
        {
            ecommerceKpi = new EcommerceKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
            kpiProxy.WriteEcommerceKpi(ecommerceKpi);
        }

        var ecommerceChannel = kpiProxy.GetEcommerceChannelByParentId(ecommerceKpi.Id) ?? new EcommerceChannel
        {
            EcommercekpiId = ecommerceKpi.Id
        };
        kpiProxy.WriteEcommerceChannel(ecommerceChannel);

        var ecommerceMobile = kpiProxy.GetEcommerceMobileByParentId(ecommerceKpi.Id) ?? new EcommerceMobile
        {
            EcommercekpiId = ecommerceKpi.Id
        };
        kpiProxy.WriteEcommerceMobile(ecommerceMobile);

        var ecommerceTotal = kpiProxy.GetEcommerceTotalByParentId(ecommerceKpi.Id) ?? new EcommerceTotal
        {
            EcommercekpiId = ecommerceKpi.Id
        };
        kpiProxy.WriteEcommerceTotal(ecommerceTotal);

        var ecommerceWebsite = kpiProxy.GetEcommerceWebsiteByParentId(ecommerceKpi.Id) ?? new EcommerceWebsite
        {
            EcommercekpiId = ecommerceKpi.Id
        };
        kpiProxy.WriteEcommerceWebsite(ecommerceWebsite);

        var generalKpi = kpiProxy.GetGeneralKpiByAdIdAndEffectiveDate(ad.Id, effectiveDate);
        if (generalKpi == null)
        {
            generalKpi = new GeneralKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
        }

        generalKpi.IsActive = (ulong)(ad.AdDeactivated == null ? 0 : 1);
        kpiProxy.WriteGeneralKpi(generalKpi);

        var leadgenKpi = kpiProxy.GetLeadgenKpiAdIdAndEffectiveDate(ad.Id, effectiveDate);
        if (leadgenKpi == null)
        {
            leadgenKpi = new LeadgenKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
            kpiProxy.WriteLeadgenKpi(leadgenKpi);
        }

        var leadgenApplication = kpiProxy.GetLeadgenApplicationByParentId(leadgenKpi.Id) ?? new LeadgenApplication
        {
            LeadgenkpiId = leadgenKpi.Id
        };
        kpiProxy.WriteLeadgenApplication(leadgenApplication);

        var leadgenAppointment = kpiProxy.GetLeadgenAppointmentByParentId(leadgenKpi.Id) ?? new LeadgenAppointment
        {
            LeadgenkpiId = leadgenKpi.Id
        };
        kpiProxy.WriteLeadgenAppointment(leadgenAppointment);

        var leadgenContact = kpiProxy.GetLeadgenContactByParentId(leadgenKpi.Id) ?? new LeadgenContact
        {
            LeadgenkpiId = leadgenKpi.Id
        };
        kpiProxy.WriteLeadgenContact(leadgenContact);

        var leadgenLead = kpiProxy.GetLeadgenLeadByParentId(leadgenKpi.Id) ?? new LeadgenLead
        {
            LeadgenkpiId = leadgenKpi.Id
        };
        kpiProxy.WriteLeadgenLead(leadgenLead);

        var leadgenLocation = kpiProxy.GetLeadgenLocationByParentId(leadgenKpi.Id) ?? new LeadgenLocation
        {
            LeadgenkpiId = leadgenKpi.Id
        };
        kpiProxy.WriteLeadgenLocation(leadgenLocation);

        var leadgenRegistration = kpiProxy.GetLeadgenRegistrationByParentId(leadgenKpi.Id) ?? new LeadgenRegistration
        {
            LeadgenkpiId = leadgenKpi.Id
        };
        kpiProxy.WriteLeadgenRegistration(leadgenRegistration);

        var leadgenSubscription = kpiProxy.GetLeadgenSubscriptionByParentId(leadgenKpi.Id) ?? new LeadgenSubscription
        {
            LeadgenkpiId = leadgenKpi.Id
        };
        kpiProxy.WriteLeadgenSubscription(leadgenSubscription);

        var leadgenTrial = kpiProxy.GetLeadgenTrialByParentId(leadgenKpi.Id) ?? new LeadgenTrial
        {
            LeadgenkpiId = leadgenKpi.Id
        };
        kpiProxy.WriteLeadgenTrial(leadgenTrial);

        var videoKpi = kpiProxy.GetVideoKpiByAdIdAndEffectiveDate(ad.Id, effectiveDate);
        if (videoKpi == null)
        {
            videoKpi = new VideoKpi
            {
                AdId = ad.Id,
                EffectiveDate = effectiveDate,
                Created = createdDate
            };
            kpiProxy.WriteVideoKpi(videoKpi);
        }

        LoadAppKpi(entry, appKpi);
        LoadEcommerceKpi(entry, ecommerceChannel, ecommerceMobile, ecommerceTotal, ecommerceWebsite);
        LoadGeneralKpi(entry, generalKpi);
        LoadLeadgenKpi(entry, leadgenApplication, leadgenAppointment, leadgenContact, leadgenLead,
            leadgenLocation, leadgenRegistration, leadgenSubscription, leadgenTrial);
        LoadVideoKpi(entry, videoKpi);

        // Final save with updated timestamp
        appKpi.Updated = createdDate;
        kpiProxy.WriteAppKpi(appKpi);

        ecommerceKpi.Updated = createdDate;
        kpiProxy.WriteEcommerceKpi(ecommerceKpi);
        kpiProxy.WriteEcommerceChannel(ecommerceChannel);
        kpiProxy.WriteEcommerceMobile(ecommerceMobile);
        kpiProxy.WriteEcommerceTotal(ecommerceTotal);
        kpiProxy.WriteEcommerceWebsite(ecommerceWebsite);

        generalKpi.Updated = createdDate;
        kpiProxy.WriteGeneralKpi(generalKpi);

        leadgenKpi.Updated = createdDate;
        kpiProxy.WriteLeadgenKpi(leadgenKpi);
        kpiProxy.WriteLeadgenApplication(leadgenApplication);
        kpiProxy.WriteLeadgenAppointment(leadgenAppointment);
        kpiProxy.WriteLeadgenContact(leadgenContact);
        kpiProxy.WriteLeadgenLead(leadgenLead);
        kpiProxy.WriteLeadgenLocation(leadgenLocation);
        kpiProxy.WriteLeadgenRegistration(leadgenRegistration);
        kpiProxy.WriteLeadgenSubscription(leadgenSubscription);
        kpiProxy.WriteLeadgenTrial(leadgenTrial);

        videoKpi.Updated = createdDate;
        kpiProxy.WriteVideoKpi(videoKpi);
    }

    private static int? ConvertStringToInt(string value)
    {
        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        }
        return null;
    }
    
    private static decimal? ConvertStringToDecimal(string value)
    {
        if (decimal.TryParse(value, out var decimalValue))
        {
            return decimalValue;
        }
        return null;
    }

    private static decimal? ConvertFloatToDecimal(float? value)
    {
        return (decimal?)value;
    }
    
    private static decimal? ConvertDoubleToDecimal(double? value)
    {
        return (decimal?)value;
    }

    private static int? ConvertFloatToInt(float? value)
    {
        return (int?)value;
    }
    
    private static void LoadGeneralKpi(FacebookInsight entry, GeneralKpi generalKpi)
    {
        generalKpi.AdRecallLift = entry.EstimatedAdRecallers.Count;
        generalKpi.AdRecallRate = null;
        generalKpi.AllClicks = entry.Clicks;
        generalKpi.AllCpc = ConvertFloatToDecimal(entry.Cpc);
        generalKpi.AllCtr = ConvertFloatToDecimal(entry.Ctr);
        generalKpi.ConversionRateRanking = entry.ConversionRateRanking;
        generalKpi.Cpm = ConvertFloatToDecimal(entry.Cpm);
        generalKpi.EngagementRanking = entry.EngagementRateRanking;
        generalKpi.Frequency = ConvertStringToDecimal(entry.Frequency);
        generalKpi.Impressions = entry.Impressions;
        generalKpi.LinkClickTotal = entry.LinkClick.Count;
        generalKpi.OutboundClicks = entry.OutboundClicks;
        generalKpi.OutboundCtr = ConvertFloatToDecimal(entry.OutboundClicksCtr);
        generalKpi.OutboundLinkClickCpc = entry.OutboundClicks;
        generalKpi.PageEngagements = entry.PageEngagement.Count;
        generalKpi.PageLikes = entry.Like.Count;
        generalKpi.PhotosViewed = entry.PhotoView.Count;
        generalKpi.PostComments = entry.Comment.Count;
        generalKpi.PostReactions = entry.PostReaction.Count;
        generalKpi.PostSaves = entry.OnsiteConversionPostSave.Count;
        generalKpi.PostShares = entry.Post.Count;
        generalKpi.QualityRanking = entry.QualityRanking;
        generalKpi.Reach = ConvertStringToInt(entry.Reach);
        generalKpi.Spend = ConvertFloatToDecimal(entry.Spend);
        generalKpi.InlineCtr = ConvertFloatToDecimal(entry.InlineLinkClickCtr);
        generalKpi.CostPerLinkClick = ConvertFloatToDecimal(entry.LinkClick.CostPerAction);
        generalKpi.LandingPageView = entry.LandingPageView.Count;
        generalKpi.WebsiteViewContent = entry.OffsiteConversionViewContent.Count;
    }

    private static void LoadAppKpi(FacebookInsight entry, AppKpi appKpi)
    {
        appKpi.AppOpen = entry.AppUse.Count;
        appKpi.Installs = entry.AppInstall.Count;
    }

    private static void LoadVideoKpi(FacebookInsight entry, VideoKpi videoKpi)
    {
        videoKpi.AverageWatchSeconds = ConvertFloatToInt(entry.VideoAvgTimeWatched);
        videoKpi.Play100percent = entry.VideoP100Watched;
        videoKpi.Play25percent = entry.VideoP25Watched;
        videoKpi.Play3seconds = entry.VideoPlay3Seconds.Count;
        videoKpi.Play50percent = entry.VideoP50Watched;
        videoKpi.Play75percent = entry.VideoP75Watched;
        videoKpi.Play95percent = entry.VideoP95Watched;
        videoKpi.PlayThru = entry.VideoThruplayWatched;
        videoKpi.PlayTotal = entry.VideoPlay;
        videoKpi.Watched30seconds = entry.Video30SecWatched;
    }

    private static void LoadEcommerceKpi(FacebookInsight entry, EcommerceChannel ecommerceChannel,
        EcommerceMobile ecommerceMobile, EcommerceTotal ecommerceTotal,
        EcommerceWebsite ecommerceWebsite)
    {
        ecommerceChannel.ChannelAddToCart = entry.AddToCart.Count;
        ecommerceChannel.ChannelAddToWishlist = entry.AddToWishlist.Count;
        ecommerceChannel.ChannelAddToWishlistValue = ConvertFloatToDecimal(entry.AddToWishlist.Value);
        ecommerceChannel.ChannelCheckoutInitiated = entry.InitiateCheckout.Count;
        ecommerceChannel.ChannelCheckoutInitiatedValue = ConvertFloatToDecimal(entry.InitiateCheckout.Value);
        ecommerceChannel.ChannelPurchases = entry.OnsitePurchases.Count;
        ecommerceChannel.ChannelPurchasesValue = ConvertFloatToDecimal(entry.OnsitePurchases.Value);

        ecommerceMobile.MobileAppAddPaymentInfo = entry.MobileAddPayment.Count;
        ecommerceMobile.MobileAppAddPaymentInfoValue = ConvertFloatToInt(entry.MobileAddPayment.Value);
        ecommerceMobile.MobileAppAddToCart = entry.MobileAddToCart.Count;
        ecommerceMobile.MobileAppAddToCartValue = ConvertFloatToDecimal(entry.MobileAddToCart.Value);
        ecommerceMobile.MobileAppAddToWishlist = entry.MobileAddToWishlist.Count;
        ecommerceMobile.MobileAppAddToWishlistValue = ConvertFloatToDecimal(entry.MobileAddToWishlist.Value);
        ecommerceMobile.MobileAppCheckoutInitiated = entry.MobileInitiatedCheckout.Count;
        ecommerceMobile.MobileAppCheckoutInitiatedValue = ConvertFloatToDecimal(entry.MobileInitiatedCheckout.Value);

        ecommerceTotal.TotalAddPaymentInfo = entry.TotalAddPaymentInfo.Count;
        ecommerceTotal.TotalAddPaymentInfoValue = ConvertFloatToDecimal(entry.TotalAddPaymentInfo.CostPerAction);
        ecommerceTotal.TotalAddToCart = entry.TotalAddToCart.Count;
        ecommerceTotal.TotalAddToCartValue = ConvertFloatToDecimal(entry.TotalAddToCart.Value);
        ecommerceTotal.TotalAddToWishlist = entry.TotalAddToWishlist.Count;
        ecommerceTotal.TotalAddToWishlistValue = ConvertFloatToDecimal(entry.TotalAddToWishlist.Value);
        ecommerceTotal.TotalCheckoutInitiated = entry.TotalCheckoutInitiated.Count;
        ecommerceTotal.TotalCheckoutInitiatedValue = ConvertFloatToDecimal(entry.TotalCheckoutInitiated.Value);
        ecommerceTotal.TotalPurchases = entry.TotalPurchases.Count;
        ecommerceTotal.TotalPurchasesValue = ConvertFloatToDecimal(entry.TotalPurchases.Value);

        ecommerceWebsite.CostPerWebsiteAddPaymentInfo = ConvertFloatToDecimal(entry.OffsiteConversionAddPayment.CostPerAction);
        ecommerceWebsite.CostPerWebsiteAddToCart = ConvertFloatToDecimal(entry.OffsiteConversionAddToCart.CostPerAction);
        ecommerceWebsite.CostPerWebsiteAddToWishlist = ConvertFloatToDecimal(entry.OffsiteConversionAddToWishlist.CostPerAction);
        ecommerceWebsite.CostPerWebsiteCheckoutInitiated = ConvertFloatToDecimal(entry.OffsiteConversionInitiateCheckout.CostPerAction);
        ecommerceWebsite.CostPerWebsitePurchases = ConvertFloatToDecimal(entry.OffsiteConversionPurchase.CostPerAction);
        ecommerceWebsite.WebsiteAddPaymentInfo = entry.OffsiteConversionAddPayment.Count;
        ecommerceWebsite.WebsiteAddPaymentInfoValue = ConvertFloatToDecimal(entry.OffsiteConversionAddPayment.Value);
        ecommerceWebsite.WebsiteAddToCart = entry.OffsiteConversionAddToCart.Count;
        ecommerceWebsite.WebsiteAddToCartValue = ConvertFloatToDecimal(entry.OffsiteConversionAddToCart.Value);
        ecommerceWebsite.WebsiteAddToWishlist = entry.OffsiteConversionAddToWishlist.Count;
        ecommerceWebsite.WebsiteAddToWishlistValue = ConvertFloatToDecimal(entry.OffsiteConversionAddToWishlist.Value);
        ecommerceWebsite.WebsiteCheckoutInitiated = entry.OffsiteConversionInitiateCheckout.Count;
        ecommerceWebsite.WebsiteCheckoutInitiatedValue = ConvertFloatToDecimal(entry.OffsiteConversionInitiateCheckout.Value);
        ecommerceWebsite.WebsitePurchases = entry.OffsiteConversionPurchase.Count;
        ecommerceWebsite.WebsitePurchasesValue = ConvertFloatToDecimal(entry.OffsiteConversionPurchase.Value);
    }

    private static void LoadLeadgenKpi(FacebookInsight entry,
        LeadgenApplication leadgenApplication,
        LeadgenAppointment leadgenAppointment,
        LeadgenContact leadgenContact,
        LeadgenLead leadgenLead,
        LeadgenLocation leadgenLocation,
        LeadgenRegistration leadgenRegistration,
        LeadgenSubscription leadgenSubscription,
        LeadgenTrial leadgenTrial)
    {
        leadgenApplication.CostPerMobileAppSubmitApplications = ConvertFloatToDecimal(entry.SubmitApplicationMobileApp.CostPerAction);
        leadgenApplication.CostPerOfflineSubmitApplications = ConvertFloatToDecimal(entry.SubmitApplicationOffline.CostPerAction);
        leadgenApplication.CostPerSubmitApplications = ConvertFloatToDecimal(entry.SubmitApplicationTotal.CostPerAction);
        leadgenApplication.CostPerWebsiteSubmitApplications = ConvertFloatToDecimal(entry.SubmitApplicationWebsite.CostPerAction);
        leadgenApplication.MobileAppSubmitApplications = entry.SubmitApplicationMobileApp.Count;
        leadgenApplication.MobileAppSubmitApplicationsValue = ConvertFloatToDecimal(entry.SubmitApplicationMobileApp.Value);
        leadgenApplication.OfflineSubmitApplications = entry.SubmitApplicationOffline.Count;
        leadgenApplication.OfflineSubmitApplicationsValue = ConvertFloatToDecimal(entry.SubmitApplicationOffline.Value);
        leadgenApplication.SubmitApplications = entry.SubmitApplicationTotal.Count;
        leadgenApplication.SubmitApplicationsValue = ConvertFloatToDecimal(entry.SubmitApplicationTotal.Value);
        leadgenApplication.WebsiteSubmitApplications = entry.SubmitApplicationWebsite.Count;
        leadgenApplication.WebsiteSubmitApplicationsValue = ConvertFloatToDecimal(entry.SubmitApplicationWebsite.Value);

        leadgenAppointment.AppointmentsScheduled = entry.ScheduleTotal.Count;
        leadgenAppointment.AppointmentsScheduledValue = ConvertFloatToDecimal(entry.ScheduleTotal.Value);
        leadgenAppointment.CostPerAppointmentScheduled = ConvertFloatToDecimal(entry.ScheduleTotal.CostPerAction);
        leadgenAppointment.CostPerMobileAppAppointmentsScheduled = ConvertFloatToDecimal(entry.ScheduleMobileApp.CostPerAction);
        leadgenAppointment.CostPerOfflineAppointmentsScheduled = ConvertFloatToDecimal(entry.ScheduleOffline.CostPerAction);
        leadgenAppointment.CostPerWebsiteAppointmentsScheduled = ConvertFloatToDecimal(entry.ScheduleWebsite.CostPerAction);
        leadgenAppointment.MobileAppAppointmentsScheduled = entry.ScheduleMobileApp.Count;
        leadgenAppointment.MobileAppAppointmentsScheduledValue = ConvertFloatToDecimal(entry.ScheduleMobileApp.Value);
        leadgenAppointment.OfflineAppointmentsScheduled = entry.ScheduleOffline.Count;
        leadgenAppointment.OfflineAppointmentsScheduledValue = ConvertFloatToDecimal(entry.ScheduleOffline.Value);
        leadgenAppointment.WebsiteAppointmentsScheduled = entry.ScheduleWebsite.Count;
        leadgenAppointment.WebsiteAppointmentsScheduledValue = ConvertFloatToDecimal(entry.ScheduleWebsite.Value);

        leadgenContact.Contacts = entry.ContactTotal.Count;
        leadgenContact.ContactsValue = ConvertFloatToDecimal(entry.ContactTotal.Value);
        leadgenContact.CostPerContacts = ConvertFloatToDecimal(entry.ContactTotal.CostPerAction);
        leadgenContact.CostPerMobileAppContacts = ConvertFloatToDecimal(entry.ContactMobileApp.CostPerAction);
        leadgenContact.CostPerOfflineContacts = ConvertFloatToDecimal(entry.ContactOffline.CostPerAction);
        leadgenContact.CostPerWebsiteContacts = ConvertFloatToDecimal(entry.ContactWebsite.CostPerAction);
        leadgenContact.MobileAppContacts = entry.ContactMobileApp.Count;
        leadgenContact.MobileAppContactsValue = ConvertFloatToDecimal(entry.ContactMobileApp.Value);
        leadgenContact.OfflineContacts = entry.ContactOffline.Count;
        leadgenContact.OfflineContactsValue = ConvertFloatToDecimal(entry.ContactOffline.Value);
        leadgenContact.WebsiteContacts = entry.ContactWebsite.Count;
        leadgenContact.WebsiteContactsValue = ConvertFloatToDecimal(entry.ContactWebsite.Value);

        leadgenLead.ChannelLeadGenFormsSubmitted = entry.Leadgen.Count;
        leadgenLead.CostPerLead = ConvertFloatToDecimal(entry.Lead.CostPerAction);
        leadgenLead.Leads = entry.Lead.Count;
        leadgenLead.WebsiteLeads = entry.Lead.Count;
        leadgenLead.WebsiteLeadsValue = ConvertFloatToDecimal(entry.Lead.Value);

        leadgenLocation.CostPerFindLocations = ConvertFloatToDecimal(entry.FindLocationTotal.CostPerAction);
        leadgenLocation.CostPerMobileAppFindLocations = ConvertFloatToDecimal(entry.FindLocationMobile.CostPerAction);
        leadgenLocation.CostPerOfflineFindLocations = ConvertFloatToDecimal(entry.FindLocationOffline.CostPerAction);
        leadgenLocation.CostPerWebsiteFindLocations = ConvertFloatToDecimal(entry.FindLocationWebsite.CostPerAction);
        leadgenLocation.FindLocations = entry.FindLocationTotal.Count;
        leadgenLocation.FindLocationsValue = ConvertFloatToDecimal(entry.FindLocationTotal.Value);
        leadgenLocation.MobileAppFindLocations = entry.FindLocationMobile.Count;
        leadgenLocation.MobileAppFindLocationsValue = ConvertFloatToDecimal(entry.FindLocationMobile.Value);
        leadgenLocation.OfflineFindLocations = entry.FindLocationOffline.Count;
        leadgenLocation.OfflineFindLocationsValue = ConvertFloatToDecimal(entry.FindLocationOffline.Value);
        leadgenLocation.WebsiteFindLocations = entry.FindLocationWebsite.Count;
        leadgenLocation.WebsiteFindLocationsValue = ConvertFloatToDecimal(entry.FindLocationWebsite.Value);

        leadgenRegistration.CostPerRegistrationsCompleted = ConvertFloatToDecimal(entry.CompleteRegistration.CostPerAction);
        leadgenRegistration.CostPerWebsiteRegistrationsCompleted = ConvertFloatToDecimal(entry.SubmitApplicationWebsite.CostPerAction);
        leadgenRegistration.MobileAppRegistrationsCompleted = ConvertFloatToInt(entry.MobileCompleteRegistration.CostPerAction);
        leadgenRegistration.MobileAppRegistrationsCompletedValue = ConvertFloatToDecimal(entry.MobileCompleteRegistration.Value);
        leadgenRegistration.RegistrationsCompleted = entry.CompleteRegistration.Count;
        leadgenRegistration.RegistrationsCompletedValue = ConvertFloatToDecimal(entry.CompleteRegistration.Value);
        leadgenRegistration.WebsiteRegistrationsCompleted = entry.SubmitApplicationWebsite.Count;
        leadgenRegistration.WebsiteRegistrationsCompletedValue = ConvertFloatToDecimal(entry.SubmitApplicationWebsite.Value);

        leadgenSubscription.CostPerMobileAppSubscriptions = ConvertFloatToDecimal(entry.SubscribeMobileApp.CostPerAction);
        leadgenSubscription.CostPerSubscriptions = ConvertFloatToDecimal(entry.SubscribeTotal.CostPerAction);
        leadgenSubscription.CostPerWebsiteSubscriptions = ConvertFloatToDecimal(entry.SubscribeWebsite.CostPerAction);
        leadgenSubscription.MobileAppSubscriptions = entry.SubscribeMobileApp.Count;
        leadgenSubscription.MobileAppSubscriptionsValue = ConvertFloatToDecimal(entry.SubscribeMobileApp.Value);
        leadgenSubscription.Subscriptions = entry.SubscribeTotal.Count;
        leadgenSubscription.SubscriptionsValue = ConvertFloatToDecimal(entry.SubscribeTotal.Value);
        leadgenSubscription.WebsiteSubscriptions = entry.SubscribeWebsite.Count;
        leadgenSubscription.WebsiteSubscriptionsValue = ConvertFloatToDecimal(entry.SubscribeWebsite.Value);

        leadgenTrial.OfflineTrialsStarted = entry.StartTrialOffline.Count;
        leadgenTrial.OfflineTrialsStartedValue = ConvertFloatToDecimal(entry.StartTrialOffline.Value);
        leadgenTrial.MobileTrialsStarted = entry.StartTrialMobileApp.Count;
        leadgenTrial.MobileTrialsStartedValue = ConvertFloatToDecimal(entry.StartTrialMobileApp.Value);
        leadgenTrial.TotalTrialsStarted = ConvertFloatToInt(entry.StartTrialTotal.Value);
        leadgenTrial.TotalTrialsStartedValue = ConvertFloatToDecimal(entry.StartTrialTotal.Value);
        leadgenTrial.WebsiteTrialsStarted = entry.StartTrialWebsite.Count;
        leadgenTrial.WebsiteTrialsStartedValue = ConvertFloatToDecimal(entry.StartTrialWebsite.Value);
    }
}