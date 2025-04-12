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
	
	LeadgenApplication? GetLeadgenApplicationByParentId(int ecommerceKpiId);
	void WriteLeadgenApplication(LeadgenApplication leadgenApplication);
	
	LeadgenAppointment? GetLeadgenAppointmentByParentId(int ecommerceKpiId);
	void WriteLeadgenAppointment(LeadgenAppointment leadgenAppointment);
	
	LeadgenContact? GetLeadgenContactByParentId(int ecommerceKpiId);
	void WriteLeadgenContact(LeadgenContact leadgenContact);
	
	LeadgenLead? GetLeadgenLeadByParentId(int ecommerceKpiId);
	void WriteLeadgenLead(LeadgenLead leadgenLe);
	
	LeadgenLocation? GetLeadgenLocationByParentId(int ecommerceKpiId);
	void WriteLeadgenLocation(LeadgenLocation leadgenLocation);
	
	LeadgenRegistration? GetLeadgenRegistrationByParentId(int ecommerceKpiId);
	void WriteLeadgenRegistration(LeadgenRegistration leadgenRegistration);
	
	LeadgenSubscription? GetLeadgenSubscriptionByParentId(int ecommerceKpiId);
	void WriteLeadgenSubscription(LeadgenSubscription leadgenSubscription);
	
	LeadgenTrial? GetLeadgenTrialByParentId(int ecommerceKpiId);
	void WriteLeadgenTrial(LeadgenTrial leadgenTrial);
	
	VideoKpi? GetVideoKpiByAdIdAndEffectiveDate(int adId, DateTime effectiveDate);
	void WriteVideoKpi(VideoKpi videoKpi);
}