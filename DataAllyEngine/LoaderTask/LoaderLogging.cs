using FacebookLoader.Common;

namespace DataAllyEngine.LoaderTask;

public class LoaderLogging : ILogging
{
	private readonly ILogger logger;

	public LoaderLogging(ILogger logger)
	{
		this.logger = logger;
	}
	
	public void LogException(Exception ex, string message) => logger.LogError(ex, message);

	public void LogError(string message) => logger.LogError(message);

	public void LogWarning(string message) => logger.LogWarning(message);

	public void LogInformation(string message) => logger.LogInformation(message);

	public void LogDebug(string message) => logger.LogDebug(message);
}