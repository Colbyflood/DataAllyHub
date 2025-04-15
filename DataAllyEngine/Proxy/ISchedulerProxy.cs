using DataAllyEngine.Models;

namespace DataAllyEngine.Proxy;

public interface ISchedulerProxy
{
	List<FbDailySchedule> GetDailySchedulesByTriggerHour(int hour);

	void WriteFbDailySchedule(FbDailySchedule schedule);
	
	Channel? GetChannelById(int channelId);

	ChannelType? GetChannelTypeByName(string channelName);
	
	Client? GetClientById(int clientId);
	
	Account? GetAccountById(int accountId);
	
	Company? GetCompanyByChannelId(int channelId);
		
	Token? GetTokenByCompanyAndChannelType(int companyId, ChannelType channelType);
	
	FbRunLog? GetFbRunLogById(int id);
	
	List<FbRunLog> GetFbRunLogsByChannelIdAfterDate(int channelId, string scopeType, DateTime date);
	
	List<FbRunLog> GetFinishedFbRunLogsAfterDate(DateTime date);
	
	List<FbRunLog> GetFbRunLogsByChannelIdAndScopeTypeInDateRange(int channelId, string scopeType, DateTime startDate, DateTime endDate);

	List<FbRunLog> GetIncompleteFbRunLogsSince(DateTime startDateUtc);
	
	void WriteFbRunLog(FbRunLog runLog);
		
	List<FbRunProblem> GetFbRunProblemsByRunlogIdOrderByDescendingCreated(int runlogId);
	
	void WriteFbRunProblem(FbRunProblem runProblem);
	
	FbSaveContent? LoadFbSaveContentContainingRunlog(int runlogId);
	
	void WriteFbSaveContent(FbSaveContent saveContent);
	
	List<FbBackfillRequest> GetBackfillRequests();
	
	void DeleteFbBackfillRequest(FbBackfillRequest request);
}