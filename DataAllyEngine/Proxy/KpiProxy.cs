using DataAllyEngine.Context;
using DataAllyEngine.Models;

namespace DataAllyEngine.Proxy;

public class KpiProxy : IKpiProxy
{
	private readonly DataAllyDbContext context;
	private readonly ILogger<ILoaderProxy> logger;
	
	public KpiProxy(DataAllyDbContext context, ILogger<ILoaderProxy> logger)
	{
		this.context = context;
		this.logger = logger;
	}
	
	public AppKpi? GetAppKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate)
	{
		return context.Appkpis.FirstOrDefault(x => x.AdId == adId && x.EffectiveDate == effectiveDate);
	}

	public void WriteAppKpi(AppKpi appKpi)
	{
		if (appKpi.Id <= 0)
		{
			context.Appkpis.Add(appKpi);
		}
		context.SaveChanges();	
	}

	public EcommerceKpi? GetEcommerceKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate)
	{
		return context.Ecommercekpis.FirstOrDefault(x => x.AdId == adId && x.EffectiveDate == effectiveDate);
	}

	public void WriteEcommerceKpi(EcommerceKpi ecommerceKpi)
	{
		if (ecommerceKpi.Id <= 0)
		{
			context.Ecommercekpis.Add(ecommerceKpi);
		}
		context.SaveChanges();
	}

	public EcommerceChannel? GetEcommerceChannelByParentId(int ecommerceKpiId)
	{
		return context.Ecommercechannels.FirstOrDefault(x => x.EcommercekpiId == ecommerceKpiId);
	}

	public void WriteEcommerceChannel(EcommerceChannel ecommerceChannel)
	{
		if (ecommerceChannel.Id <= 0)
		{
			context.Ecommercechannels.Add(ecommerceChannel);
		}
		context.SaveChanges();
	}

	public EcommerceMobile? GetEcommerceMobileByParentId(int ecommerceKpiId)
	{
		return context.Ecommercemobiles.FirstOrDefault(x => x.EcommercekpiId == ecommerceKpiId);
	}

	public void WriteEcommerceMobile(EcommerceMobile ecommerceMobile)
	{
		if (ecommerceMobile.Id <= 0)
		{
			context.Ecommercemobiles.Add(ecommerceMobile);
		}
		context.SaveChanges();
	}

	public EcommerceTotal? GetEcommerceTotalByParentId(int ecommerceKpiId)
	{
		return context.Ecommercetotals.FirstOrDefault(x => x.EcommercekpiId == ecommerceKpiId);
	}

	public void WriteEcommerceTotal(EcommerceTotal ecommerceTotal)
	{
		if (ecommerceTotal.Id <= 0)
		{
			context.Ecommercetotals.Add(ecommerceTotal);
		}
		context.SaveChanges();
	}

	public EcommerceWebsite? GetEcommerceWebsiteByParentId(int ecommerceKpiId)
	{
		return context.Ecommercewebsites.FirstOrDefault(x => x.EcommercekpiId == ecommerceKpiId);
	}

	public void WriteEcommerceWebsite(EcommerceWebsite ecommerceWebsite)
	{
		if (ecommerceWebsite.Id <= 0)
		{
			context.Ecommercewebsites.Add(ecommerceWebsite);
		}
		context.SaveChanges();
	}

	public GeneralKpi? GetGeneralKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate)
	{
		return context.Generalkpis.FirstOrDefault(x => x.AdId == adId && x.EffectiveDate == effectiveDate);
	}

	public void WriteGeneralKpi(GeneralKpi generalKpi)
	{
		if (generalKpi.Id <= 0)
		{
			context.Generalkpis.Add(generalKpi);
		}
		context.SaveChanges();
	}

	public LeadgenKpi? GetLeadgenKpiAdIdAndEffectiveDate(int adId, DateTime effectiveDate)
	{
		return context.Leadgenkpis.FirstOrDefault(x => x.AdId == adId && x.EffectiveDate == effectiveDate);
	}

	public void WriteLeadgenKpi(LeadgenKpi leadgenKpi)
	{
		if (leadgenKpi.Id <= 0)
		{
			context.Leadgenkpis.Add(leadgenKpi);
		}
		context.SaveChanges();
	}

	public LeadgenApplication? GetLeadgenApplicationByParentId(int leadgenKpiId)
	{
		return context.Leadgenapplications.FirstOrDefault(x => x.LeadgenkpiId == leadgenKpiId);
	}

	public void WriteLeadgenApplication(LeadgenApplication leadgenApplication)
	{
		if (leadgenApplication.Id <= 0)
		{
			context.Leadgenapplications.Add(leadgenApplication);
		}
		context.SaveChanges();
	}

	public LeadgenAppointment? GetLeadgenAppointmentByParentId(int leadgenKpiId)
	{
		return context.Leadgenappointments.FirstOrDefault(x => x.LeadgenkpiId == leadgenKpiId);
	}

	public void WriteLeadgenAppointment(LeadgenAppointment leadgenAppointment)
	{
		if (leadgenAppointment.Id <= 0)
		{
			context.Leadgenappointments.Add(leadgenAppointment);
		}
		context.SaveChanges();
	}

	public LeadgenContact? GetLeadgenContactByParentId(int leadgenKpiId)
	{
		return context.Leadgencontacts.FirstOrDefault(x => x.LeadgenkpiId == leadgenKpiId);
	}

	public void WriteLeadgenContact(LeadgenContact leadgenContact)
	{
		if (leadgenContact.Id <= 0)
		{
			context.Leadgencontacts.Add(leadgenContact);
		}
		context.SaveChanges();
	}

	public LeadgenLead? GetLeadgenLeadByParentId(int leadgenKpiId)
	{
		return context.Leadgenleads.FirstOrDefault(x => x.LeadgenkpiId == leadgenKpiId);
	}

	public void WriteLeadgenLead(LeadgenLead leadgenLead)
	{
		if (leadgenLead.Id <= 0)
		{
			context.Leadgenleads.Add(leadgenLead);
		}
		context.SaveChanges();
	}

	public LeadgenLocation? GetLeadgenLocationByParentId(int leadgenKpiId)
	{
		return context.Leadgenlocations.FirstOrDefault(x => x.LeadgenkpiId == leadgenKpiId);
	}

	public void WriteLeadgenLocation(LeadgenLocation leadgenLocation)
	{
		if (leadgenLocation.Id <= 0)
		{
			context.Leadgenlocations.Add(leadgenLocation);
		}
		context.SaveChanges();
	}

	public LeadgenRegistration? GetLeadgenRegistrationByParentId(int leadgenKpiId)
	{
		return context.Leadgenregistrations.FirstOrDefault(x => x.LeadgenkpiId == leadgenKpiId);
	}

	public void WriteLeadgenRegistration(LeadgenRegistration leadgenRegistration)
	{
		if (leadgenRegistration.Id <= 0)
		{
			context.Leadgenregistrations.Add(leadgenRegistration);
		}
		context.SaveChanges();
	}

	public LeadgenSubscription? GetLeadgenSubscriptionByParentId(int leadgenKpiId)
	{
		return context.Leadgensubscriptions.FirstOrDefault(x => x.LeadgenkpiId == leadgenKpiId);
	}

	public void WriteLeadgenSubscription(LeadgenSubscription leadgenSubscription)
	{
		if (leadgenSubscription.Id <= 0)
		{
			context.Leadgensubscriptions.Add(leadgenSubscription);
		}
		context.SaveChanges();
	}

	public LeadgenTrial? GetLeadgenTrialByParentId(int leadgenKpiId)
	{
		return context.Leadgentrials.FirstOrDefault(x => x.LeadgenkpiId == leadgenKpiId);
	}

	public void WriteLeadgenTrial(LeadgenTrial leadgenTrial)
	{
		if (leadgenTrial.Id <= 0)
		{
			context.Leadgentrials.Add(leadgenTrial);
		}
		context.SaveChanges();
	}

	public VideoKpi? GetVideoKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate)
	{
		return context.Videokpis.FirstOrDefault(x => x.AdId == adId && x.EffectiveDate == effectiveDate);
	}

	public void WriteVideoKpi(VideoKpi videoKpi)
	{
		if (videoKpi.Id <= 0)
		{
			context.Videokpis.Add(videoKpi);
		}
		context.SaveChanges();
	}
}