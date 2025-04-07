namespace FacebookLoader.Common;

public interface ILogging
{
	void LogError(string message);
	void LogWarning(string message);
	void LogInformation(string message);
	void LogDebug(string message);
}