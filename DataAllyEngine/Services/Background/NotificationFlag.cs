namespace DataAllyEngine.Services.Background;

public class NotificationFlag
{
	private bool flagState;

	public NotificationFlag(bool initialState)
	{
		this.flagState = initialState;
	}

	public void Set() => flagState = true;

	public void UnSet() => flagState = false;
	
	public bool IsSet() => flagState;
}