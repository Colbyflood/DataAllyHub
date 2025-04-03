namespace MakeMyCapServer.Configuration.Exceptions;

public class ConfigurationKeyNotFoundException : Exception
{
	public ConfigurationKeyNotFoundException(string key) : base(key)
	{
		
	}
}