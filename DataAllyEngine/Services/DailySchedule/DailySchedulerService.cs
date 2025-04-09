using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.DailySchedule;

namespace DataAllyEngine.Schedule;

public class DailySchedulerService : IDailySchedulerService
{
	private const int ONE_MINUTE = 1;
	private const int ONE_MINUTE_MSEC = ONE_MINUTE * 60 * 1000;
	
	private readonly ISchedulerProxy schedulerProxy;
	private readonly ILogger<DailySchedulerService> logger;

	public DailySchedulerService(ISchedulerProxy schedulerProxy, ILogger<DailySchedulerService> logger)
	{
		this.schedulerProxy = schedulerProxy;
		this.logger = logger;
	}
	
	public async Task DoWorkAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		logger.LogInformation("{ServiceName} working", nameof(DailySchedulerService));

		while (!stoppingToken.IsCancellationRequested)
		{
			RunPendingDailyServices();
			await Task.Delay(ONE_MINUTE_MSEC, stoppingToken);
		}
	}

	private void RunPendingDailyServices()
	{
		var now = DateTime.UtcNow;
		var candidates = GetPendingSchedulesFor(now);
		candidates.ForEach(candidate =>
		{
			
		});

	}

	private List<FbDailySchedule> GetPendingSchedulesFor(DateTime utcNow)
	{
		var possibles = schedulerProxy.GetDailySchedulesByTriggerHour(utcNow.Hour);
		if (possibles.Count == 0)
		{
			return possibles;
		}
		
		var candidates = new List<FbDailySchedule>();
		possibles.ForEach(possible =>
		{
			if (!schedulerProxy.GetFbRunLogsByChannelIdAfterDate(possible.ChannelId, utcNow).Any())
			{
				candidates.Add(possible);
			}
		});
		return candidates;
	}
}