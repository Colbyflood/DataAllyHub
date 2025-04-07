namespace FacebookLoader.Common;

public interface ILogging
{
	void LogException(Exception ex, string message);
	void LogError(string message);
	void LogWarning(string message);
	void LogInformation(string message);
	void LogDebug(string message);
}