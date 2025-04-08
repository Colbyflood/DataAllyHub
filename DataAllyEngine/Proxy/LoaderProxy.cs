using DataAllyEngine.Context;
using DataAllyEngine.Models;

namespace DataAllyEngine.Proxy;

public class LoaderProxy : ILoaderProxy
{
	private readonly DataAllyDbContext context;
	private readonly ILogger<ILoaderProxy> logger;
	
	public LoaderProxy(DataAllyDbContext context, ILogger<ILoaderProxy> logger)
	{
		this.context = context;
		this.logger = logger;
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

	public Token? GetTokenByCompanyAndChannelType(int companyId, ChannelType channelType)
	{
		return context.Tokens.SingleOrDefault(record => record.CompanyId == companyId && record.ChannelTypeId == channelType.Id);
	}

	public FbDailySchedule? GetFbDailyScheduleByChannelId(int channelId)
	{
		return context.Fbdailyschedules.SingleOrDefault(record => record.ChannelId == channelId);
	}

	public void WriteFbDailySchedule(FbDailySchedule dailySchedule)
	{
		if (dailySchedule.Id <= 0)
		{
			context.Fbdailyschedules.Add(dailySchedule);
		}
		context.SaveChanges();
	}

	public List<FbRunLog> GetFbRunLogsByChannelIdAfterDate(int channelId, DateTime date)
	{
		return context.Fbrunlogs.Where(record => record.ChannelId == channelId && record.StartedUtc >= date).ToList();
	}

	public void WriteFbRunLog(FbRunLog runLog)
	{
		if (runLog.Id <= 0)
		{
			context.Fbrunlogs.Add(runLog);
		}
		context.SaveChanges();
	}

	public void WriteFbSaveContent(FbSaveContent saveContent)
	{
		if (saveContent.Id <= 0)
		{
			context.Fbsavecontents.Add(saveContent);
		}
		context.SaveChanges();
	}

	public void WriteFbRunProblem(FbRunProblem runProblem)
	{
		if (runProblem.Id <= 0)
		{
			context.Fbrunproblems.Add(runProblem);
		}
		context.SaveChanges();	
	}
}