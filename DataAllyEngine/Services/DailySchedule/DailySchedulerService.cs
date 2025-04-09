using DataAllyEngine.Services.DailySchedule;

namespace DataAllyEngine.Schedule;

public class DailySchedulerService : IDailySchedulerService
{
	public Task DoWorkAsync(CancellationToken stoppingToken)
	{
		// TODO: wake every minute on the minute and check for scheduling
		throw new NotImplementedException();
	}
}