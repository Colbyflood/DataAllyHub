using DataAllyEngine.LoaderTask;
using DataAllyEngine.Proxy;
using DataAllyEngine.Services.DailySchedule;

namespace DataAllyEngine.Services.ProcessContent;

public class ProcessContentService : IProcessContentService
{
	public ProcessContentService(IContentProcessorProxy contentProcessorProxy, IKpiProxy kpiProxy, ILoaderRunner loaderRunner, ILogger<DailySchedulerService> logger)
	{
		this.schedulerProxy = schedulerProxy;
		this.loaderRunner = loaderRunner;
		this.logger = logger;
	}
}