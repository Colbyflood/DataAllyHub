namespace DataAllyEngine.Services.Notification;

public interface IStatusNotificationService
{
	void SendServiceStartupStatus(string serviceName);
	void SendServiceShutdownStatus(string serviceName);
}