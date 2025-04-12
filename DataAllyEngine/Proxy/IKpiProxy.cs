using DataAllyEngine.Models;

namespace DataAllyEngine.Proxy;

public interface IKpiProxy
{
	AppKpi? GetAppKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate);
	void WriteAppKpi(AppKpi appKpi);
	
	EcommerceKpi? GetEcommerceKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate);
	void WriteEcommerceKpi(EcommerceKpi ecommerceKpi);
	
	EcommerceChannel? GetEcommerceChannelByParentId(int ecommerceKpiId);
	void WriteEcommerceChannel(EcommerceChannel ecommerceChannel);
	
	EcommerceMobile? GetEcommerceMobileByParentId(int ecommerceKpiId);
	void WriteEcommerceMobile(EcommerceMobile ecommerceMobile);
	
	EcommerceTotal? GetEcommerceTotalByParentId(int ecommerceKpiId);
	void WriteEcommerceTotal(EcommerceTotal ecommerceTotal);
	
	EcommerceWebsite? GetEcommerceWebsiteByParentId(int ecommerceKpiId);
	void WriteEcommerceWebsite(EcommerceWebsite ecommerceWebsite);
	
	GeneralKpi? GetGeneralKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate);
	void WriteGeneralKpi(GeneralKpi generalKpi);
	
	LeadgenKpi? GetLeadgenKpiAdIdAndEffectiveDate(int adId, DateTime effectiveDate);
	void WriteLeadgenKpi(LeadgenKpi leadgenKpi);
	
	LeadgenApplication? GetLeadgenApplicationByParentId(int leadgenKpiId);
	void WriteLeadgenApplication(LeadgenApplication leadgenApplication);
	
	LeadgenAppointment? GetLeadgenAppointmentByParentId(int leadgenKpiId);
	void WriteLeadgenAppointment(LeadgenAppointment leadgenAppointment);
	
	LeadgenContact? GetLeadgenContactByParentId(int leadgenKpiId);
	void WriteLeadgenContact(LeadgenContact leadgenContact);
	
	LeadgenLead? GetLeadgenLeadByParentId(int leadgenKpiId);
	void WriteLeadgenLead(LeadgenLead leadgenLead);
	
	LeadgenLocation? GetLeadgenLocationByParentId(int leadgenKpiId);
	void WriteLeadgenLocation(LeadgenLocation leadgenLocation);
	
	LeadgenRegistration? GetLeadgenRegistrationByParentId(int leadgenKpiId);
	void WriteLeadgenRegistration(LeadgenRegistration leadgenRegistration);
	
	LeadgenSubscription? GetLeadgenSubscriptionByParentId(int leadgenKpiId);
	void WriteLeadgenSubscription(LeadgenSubscription leadgenSubscription);
	
	LeadgenTrial? GetLeadgenTrialByParentId(int leadgenKpiId);
	void WriteLeadgenTrial(LeadgenTrial leadgenTrial);
	
	VideoKpi? GetVideoKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate);
	void WriteVideoKpi(VideoKpi videoKpi);
}