using DataAllyEngine.Context;
using DataAllyEngine.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Proxy;

public class SchedulerProxy : ISchedulerProxy
{
	private readonly DataAllyDbContext context;
	private readonly ILogger<ILoaderProxy> logger;
	
	public SchedulerProxy(DataAllyDbContext context, ILogger<ILoaderProxy> logger)
	{
		this.context = context;
		this.logger = logger;
	}

	public List<FbDailySchedule> GetDailySchedulesByTriggerHour(int hour)
	{
		return context.Fbdailyschedules.Where(record => record.TriggerHourUtc == hour).ToList();
	}

	public void WriteFbDailySchedule(FbDailySchedule schedule)
	{
		if (schedule.Id <= 0)
		{
			context.Fbdailyschedules.Add(schedule);
		}
		context.SaveChanges();
	}

	public Channel? GetChannelById(int channelId)
	{
		return context.Channels.SingleOrDefault(record => record.Id == channelId);
	}
	
	public ChannelType? GetChannelTypeByName(string channelName)
	{
		return context.Channeltypes.SingleOrDefault(record => record.Name.ToLower() == channelName.ToLower());
	}
	
	public Client? GetClientById(int clientId)
	{
		return context.Clients.SingleOrDefault(record => record.Id == clientId);
	}

	public Account? GetAccountById(int accountId)
	{
		return context.Accounts.SingleOrDefault(record => record.Id == accountId);
	}

	public Company? GetCompanyByChannelId(int channelId)
	{
		var channel = context.Channels.SingleOrDefault(record => record.Id == channelId);
		if (channel == null)
		{
			return null;
		}
		var client = context.Clients.SingleOrDefault(record => record.Id == channel.ClientId);
		if (client == null)
		{
			return null;
		}
		var account = context.Accounts.SingleOrDefault(record => record.Id == client.AccountId);
		if (account == null)
		{
			return null;
		}
		return context.Companies.SingleOrDefault(record => record.Id == account.CompanyId);
	}
	
	public Token? GetTokenByCompanyAndChannelType(int companyId, ChannelType channelType)
	{
		return context.Tokens.SingleOrDefault(record => record.CompanyId == companyId && record.ChannelTypeId == channelType.Id);
	}

	public FbRunLog? GetFbRunLogById(int id)
	{
		return context.Fbrunlogs.SingleOrDefault(record => record.Id == id);
	}
	
	public List<FbRunLog> GetFbRunLogsByChannelIdAfterDate(int channelId, string scopeType, DateTime date)
	{
		return context.Fbrunlogs
			.Where(record => record.ChannelId == channelId && record.ScopeType.ToLower() == scopeType.ToLower() && record.StartedUtc >= date)
			.ToList();
	}

	public List<FbRunLog> GetUncachedFinishedFbRunLogsAfterDate(DateTime date)
	{
		return context.Fbrunlogs
			.AsNoTracking()
			.Where(record => record.StartedUtc >= date && record.FinishedUtc != null)
			.ToList();
	}

	public List<FbRunLog> GetFbRunLogsByChannelIdAndScopeTypeInDateRange(int channelId, string scopeType, DateTime startDate, DateTime endDate)
	{
		return context.Fbrunlogs
			.Where(record => record.ChannelId == channelId && record.ScopeType.ToLower() == scopeType.ToLower() && record.StartedUtc >= startDate && record.StartedUtc < endDate)
			.ToList();
	}

	public List<FbRunLog> GetUncachedIncompleteFbRunLogsSince(DateTime startDateUtc)
	{
		return context.Fbrunlogs
			.AsNoTracking()
			.Where(record => record.StartedUtc >= startDateUtc && record.FinishedUtc == null)
			.OrderBy(record => record.StartedUtc)
			.ToList();
	}
	
	public void WriteFbRunLog(FbRunLog runLog)
	{
		if (runLog.Id <= 0)
		{
			context.Fbrunlogs.Add(runLog);
		}
		context.SaveChanges();
	}

	public FbRunProblem? GetFbRunProblemById(int runProblemId)
	{
		return context.Fbrunproblems.SingleOrDefault(record => record.Id == runProblemId);
	}
	
	public List<FbRunProblem> GetUncachedFbRunProblemsByRunlogIdOrderByDescendingCreated(int runlogId)
	{
		return context.Fbrunproblems
			.AsNoTracking()
			.Where(record => record.FbRunlogId == runlogId)
			.OrderByDescending(record => record.CreatedUtc)
			.ToList();
	}
	
	public void WriteFbRunProblem(FbRunProblem runProblem)
	{
		if (runProblem.Id <= 0)
		{
			context.Fbrunproblems.Add(runProblem);
		}
		context.SaveChanges();	
	}
	
	public FbSaveContent? LoadFbSaveContentContainingRunlog(int runlogId)
	{
		return context.Fbsavecontents.FirstOrDefault(f => f.AdCreativeRunlogId == runlogId || f.AdImageRunlogId == runlogId || f.AdInsightRunlogId == runlogId);
	}

	public void WriteFbSaveContent(FbSaveContent saveContent)
	{
		if (saveContent.Id <= 0)
		{
			context.Fbsavecontents.Add(saveContent);
		}
		context.SaveChanges();
	}

	public List<FbBackfillRequest> GetBackfillRequests()
	{
		return context.Fbbackfillrequests.ToList();
	}

	public void DeleteFbBackfillRequest(FbBackfillRequest request)
	{
		context.Fbbackfillrequests.Remove(request);
		context.SaveChanges();
	}
}