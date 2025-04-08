using DataAllyEngine.Models;

namespace DataAllyEngine.Proxy;

public class ILoaderProxy
{
	Channel GetChannelById(int channelId);
	
	FbDailySchedule GetDailyScheduleByChannelId(int channelId);
}