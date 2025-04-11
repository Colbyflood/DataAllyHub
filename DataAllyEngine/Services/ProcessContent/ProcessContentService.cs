namespace DataAllyEngine.Services.ProcessContent;

public class ProcessContentService : IProcessContentService
{
	public ProcessContentService(IContentProcessorProxy contentProcessorProxy, ILoaderRunner loaderRunner, ILogger<DailySchedulerService> logger)
	{
		this.schedulerProxy = schedulerProxy;
		this.loaderRunner = loaderRunner;
		this.logger = logger;
	}
}