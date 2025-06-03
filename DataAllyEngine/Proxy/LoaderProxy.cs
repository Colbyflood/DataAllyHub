using DataAllyEngine.Common;
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

	public Channel? GetChannelByChannelAccountId(string channelAccountId)
	{
		var channelAccount = channelAccountId;
		if (!channelAccount.StartsWith("act_"))
		{
			channelAccount = $"act_{channelAccount}";
		}
		return context.Channels.SingleOrDefault(record => record.ChannelAccountId == channelAccount);
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

	public FbRunLog? GetFbRunLogById(int runlogId)
	{
		return context.Fbrunlogs.SingleOrDefault(record => record.Id == runlogId);
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

	public int GetNextSequenceByRunlogId(int runlogId)
	{
		var maxSequence = context.Fbrunstagings
			.Where(record => record.FbRunlogId == runlogId)
			.Max(record => (int?)record.Sequence) ?? 0;
		return maxSequence + 1;
	}

	public void WriteFbRunStaging(FbRunStaging runStaging)
	{
		if (runStaging.Id <= 0)
		{
			context.Fbrunstagings.Add(runStaging);
		}
		context.SaveChanges();	
	}

	public Token? GetTokenByCompanyIdAndChannelTypeId(int companyId, int channelTypeId)
	{
		return context.Tokens.SingleOrDefault(record => record.CompanyId == companyId && record.ChannelTypeId == channelTypeId);
	}

	public List<FbCreativeLoad> GetPendingCreativeImages(int startId, int batchSize)
	{
		return context.Fbcreativeloads
			.Where(rec => rec.Id > startId && rec.CreativeType == Names.CREATIVE_TYPE_IMAGE)
			.OrderBy(rec => rec.Id)
			.Take(batchSize)
			.ToList();
	}

	public List<FbCreativeLoad> GetPendingCreativeVideos(int startId, int batchSize)
	{
		return context.Fbcreativeloads
			.Where(rec => rec.Id > startId && rec.CreativeType == Names.CREATIVE_TYPE_VIDEO)
			.OrderBy(rec => rec.Id)
			.Take(batchSize)
			.ToList();
	}
}