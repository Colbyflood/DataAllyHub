using DataAllyEngine.Models;

namespace DataAllyEngine.Proxy;

public interface ILoaderProxy
{
	Channel? GetChannelById(int channelId);
	
	ChannelType? GetChannelTypeByName(string channelName);
	
	FbDailySchedule? GetFbDailyScheduleByChannelId(int channelId);
	void WriteFbDailySchedule(FbDailySchedule dailySchedule);
	
	FbRunLog? GetFbRunLogById(int runlogId);
	List<FbRunLog> GetFbRunLogsByChannelIdAfterDate(int channelId, DateTime date);
	void WriteFbRunLog(FbRunLog runLog);
	
	void WriteFbSaveContent(FbSaveContent saveContent);
	
	void WriteFbRunProblem(FbRunProblem runProblem);
	
	int GetNextSequenceByRunlogId(int runlogId);
	void WriteFbRunStaging(FbRunStaging runStaging);
}