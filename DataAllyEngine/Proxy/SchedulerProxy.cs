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

    public List<FbDailySchedule> GetDailySchedulesByTriggerHour(DateTime utcNowHour)
    {
        return context.Fbdailyschedules
                        .AsNoTracking()
                        .Where(record =>
                                    record.TriggerHourUtc == utcNowHour.Hour
                                    &&
                                    (record.LastStartedUtc == null || record.LastStartedUtc < utcNowHour) // Only fetch those which are not started yet or started before this hour
                                    )
                        .OrderBy(record => record.LastStartedUtc)
                        .ToList();
    }

    public FbDailySchedule? GetFbDailyScheduleById(int id)
    {
        return context.Fbdailyschedules.SingleOrDefault(record => record.Id == id);
    }

    public void WriteFbDailySchedule(FbDailySchedule schedule)
    {
        if (schedule.Id <= 0)
        {
            context.Fbdailyschedules.Add(schedule);
        }
        context.SaveChanges();
    }

    public Channel? GetChannelById(int channelId, bool includeAccount = false)
    {
        if (includeAccount)
        {
            return context.Channels
                          .Include(channel => channel.Client)
                          .ThenInclude(client => client.Account)
                          .SingleOrDefault(record => record.Id == channelId);
        }
        else
        {
            return context.Channels
                          .SingleOrDefault(record => record.Id == channelId);
        }
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

    /// <summary>
    /// Fetch those runlogs which are finished and started after date and ignore those whose Account is inactive.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
	public List<FbRunLog> GetUncachedFinishedFbRunLogsAfterDate(DateTime date)
    {
        return context.Fbrunlogs
            .AsNoTracking()
            .Where(record => record.StartedUtc >= date && record.FinishedUtc != null
                            && record.Channel.Client.Account.Active != false // Ignore those whose Account is inactive, Channel \ Client \ Account references can be null
            )
            .ToList();
    }

    public List<FbRunLog> GetFbRunLogsByChannelIdAndScopeTypeInDateRange(int channelId, string scopeType, DateTime startDate, DateTime endDate)
    {
        return context.Fbrunlogs
            .Where(record => record.ChannelId == channelId && record.ScopeType.ToLower() == scopeType.ToLower() && record.StartedUtc >= startDate && record.StartedUtc < endDate)
            .ToList();
    }

    /// <summary>
    /// Fetch those runlogs which are not finished and started after startedDateUtc
    /// </summary>
    /// <param name="startDateUtc"></param>
    /// <returns></returns>
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

    public FbSaveContent? LoadFbSaveContentById(int id)
    {
        return context.Fbsavecontents.FirstOrDefault(f => f.Id == id);
    }

    public FbSaveContent? GetFbSaveContentByRunlogsIds(int? fbCreativeRunlogId, int? fbImageRunlogId, int? fbInsightRunlogId)
    {
        if (fbCreativeRunlogId is null && fbImageRunlogId is null & fbInsightRunlogId is null)
            return null;

        var query = context.Fbsavecontents.AsQueryable();

        if (fbCreativeRunlogId != null)
            query = query.Where(f => f.AdCreativeRunlogId == fbCreativeRunlogId);

        if (fbImageRunlogId != null)
            query = query.Where(f => f.AdImageRunlogId == fbImageRunlogId);

        if (fbInsightRunlogId != null)
            query = query.Where(f => f.AdInsightRunlogId == fbInsightRunlogId);

        return query.FirstOrDefault();
    }

    /// <summary>
    /// Fetch those fbSaveContents whose fbRunLogs are finished and started after date and ignore those whose Account is inactive.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
	public List<FbSaveContent> GetPendingFinishedFbRunLogsSaveContentsAfterDate(DateTime date, int maxAttempts)
    {
        var query = context.Fbsavecontents
            .Where(record =>
                            (record.AdCreativeRunlogId != null && record.AdCreativeRunlog.FinishedUtc != null)
                            && (record.AdImageRunlogId != null && record.AdImageRunlog.FinishedUtc != null)
                            && (record.AdInsightRunlogId != null && record.AdInsightRunlog.FinishedUtc != null)
                            && (record.AdCreativeFinishedUtc == null || record.AdImageFinishedUtc == null || record.AdInsightFinishedUtc == null)
                            && (record.QueuedUtc == null || record.QueuedUtc >= date)
                            && (record.Attempts <= maxAttempts)
                            && (record.AdCreativeRunlog.Channel.Client.Account.Active != false) // Ignore those whose Account is inactive, Channel \ Client \ Account references can be null
            )
            .OrderBy(record => record.Attempts) // Those which are not processed even should be give first chance
            .ThenBy(record => record.QueuedUtc);

#if DEBUG
        string queryInString = query.ToQueryString();
#endif

        return query.ToList();
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